/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace OpenSearch.Net;

/// <summary>
/// A collection of handy baked in server certificate validation callbacks
/// </summary>
public static class CertificateValidations
{
    /// <summary>
    /// DANGEROUS, never use this in production validates ALL certificates to true.
    /// </summary>
    /// <returns>Always true, allowing ALL certificates</returns>
    public static bool AllowAll(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) => true;

    /// <summary>
    /// Always false, in effect blocking ALL certificates
    /// </summary>
    /// <returns>Always false, always blocking ALL certificates</returns>
    public static bool DenyAll(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) => false;

    /// <summary>
    /// Helper to create a certificate validation callback based on the certificate authority certificate that we used to
    /// generate the nodes certificates with. This callback expects the CA to be part of the chain as intermediate CA.
    /// </summary>
    /// <param name="caCertificate">The ca certificate used to generate the nodes certificate </param>
    /// <param name="trustRoot">
    /// Custom CA are never trusted by default unless they are in the machines trusted store, set this to true
    /// if you've added the CA to the machines trusted store. In which case UntrustedRoot should not be accepted.
    /// </param>
    /// <param name="revocationMode">By default we do not check revocation, it is however recommended to check this (either offline or online).</param>
    public static Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> AuthorityPartOfChain(
        X509Certificate caCertificate, bool trustRoot = true, X509RevocationMode revocationMode = X509RevocationMode.NoCheck
    ) =>
        (sender, cert, chain, errors) =>
            errors == SslPolicyErrors.None
            || ValidIntermediateCa(caCertificate, cert, chain, trustRoot, revocationMode);

    /// <summary>
    /// Helper to create a certificate validation callback based on the certificate authority certificate that we used to
    /// generate the nodes certificates with. This callback does NOT expect the CA to be part of the chain presented by the server.
    /// Including the root certificate in the chain increases the SSL handshake size and OpenSearch's certgen by default does not include
    /// the CA in the certificate chain.
    /// </summary>
    /// <param name="caCertificate">The ca certificate used to generate the nodes certificate </param>
    /// <param name="trustRoot">
    /// Custom CA are never trusted by default unless they are in the machines trusted store, set this to true
    /// if you've added the CA to the machines trusted store. In which case UntrustedRoot should not be accepted.
    /// </param>
    /// <param name="revocationMode">By default we do not check revocation, it is however recommended to check this (either offline or online).</param>
    public static Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> AuthorityIsRoot(
        X509Certificate caCertificate, bool trustRoot = true, X509RevocationMode revocationMode = X509RevocationMode.NoCheck
    ) =>
        (sender, cert, chain, errors) =>
            errors == SslPolicyErrors.None
            || ValidRootCa(caCertificate, cert, trustRoot, revocationMode);

    private static bool ValidRootCa(X509Certificate caCertificate, X509Certificate certificate, bool trustRoot,
        X509RevocationMode revocationMode
    )
    {
        var ca = new X509Certificate2(caCertificate);
        var privateChain = new X509Chain { ChainPolicy = { RevocationMode = revocationMode } };
        privateChain.ChainPolicy.ExtraStore.Add(ca);
        privateChain.Build(new X509Certificate2(certificate));

        //lets validate the our chain status
        foreach (var chainStatus in privateChain.ChainStatus)
        {
            //custom CA's that are not in the machine trusted store will always have this status
            //by setting trustRoot = true (default) we skip this error
            if (chainStatus.Status == X509ChainStatusFlags.UntrustedRoot && trustRoot) continue;

            //trustRoot is false so we expected our CA to be in the machines trusted store
            if (chainStatus.Status == X509ChainStatusFlags.UntrustedRoot) return false;

            //otherwise if the chain has any error of any sort return false
            if (chainStatus.Status != X509ChainStatusFlags.NoError) return false;
        }
        return true;
    }

    private static bool ValidIntermediateCa(X509Certificate caCertificate, X509Certificate certificate, X509Chain chain, bool trustRoot,
        X509RevocationMode revocationMode
    )
    {
        var ca = new X509Certificate2(caCertificate);
        var privateChain = new X509Chain { ChainPolicy = { RevocationMode = revocationMode } };
        privateChain.ChainPolicy.ExtraStore.Add(ca);
        privateChain.Build(new X509Certificate2(certificate));

        //Assert our chain has the same number of elements as the certifcate presented by the server
        if (chain.ChainElements.Count != privateChain.ChainElements.Count) return false;

        //lets validate the our chain status
        foreach (var chainStatus in privateChain.ChainStatus)
        {
            //custom CA's that are not in the machine trusted store will always have this status
            //by setting trustRoot = true (default) we skip this error
            if (chainStatus.Status == X509ChainStatusFlags.UntrustedRoot && trustRoot) continue;

            //trustRoot is false so we expected our CA to be in the machines trusted store
            if (chainStatus.Status == X509ChainStatusFlags.UntrustedRoot) return false;

            //otherwise if the chain has any error of any sort return false
            if (chainStatus.Status != X509ChainStatusFlags.NoError) return false;
        }

        var found = false;
        //We are going to walk both chains and make sure the thumbprints align
        //while making sure one of the chains certificates presented by the server has our expected CA thumbprint
        for (var i = 0; i < chain.ChainElements.Count; i++)
        {
            var c = chain.ChainElements[i].Certificate.Thumbprint;
            var cPrivate = privateChain.ChainElements[i].Certificate.Thumbprint;
            if (!found && c == ca.Thumbprint) found = true;

            //mis aligned certificate chain, return false so we do not accept this certificate
            if (c != cPrivate) return false;
        }
        return found;
    }
}
