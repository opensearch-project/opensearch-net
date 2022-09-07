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

using OpenSearch.Stack.ArtifactsApi;

namespace OpenSearch.OpenSearch.Ephemeral.Tasks.InstallationTasks
{
	internal class InitialConfigurationOpenSearch
	{
		// Actually, it is content of file `opensearch-tar-install.sh` shipped
		// in the tarball, but due to (1) this file might be changed and
		// (2) we have to modify the file before execution, because it launches
		// the server what we want to do on our own, it is decided to have a
		// snapshot of this file.
		// The script is taken from v.1.2.4, last 3 lines omitted.
		public static string GetConfigurationScript(OpenSearchVersion version)
		{
			if (version < (OpenSearchVersion)"2.0.0")
				return
@"#!/bin/bash

# Copyright OpenSearch Contributors
# SPDX-License-Identifier: Apache-2.0

OPENSEARCH_HOME=`dirname $(realpath $0)`; cd $OPENSEARCH_HOME
KNN_LIB_DIR=$OPENSEARCH_HOME/plugins/opensearch-knn/knnlib
##Security Plugin
bash $OPENSEARCH_HOME/plugins/opensearch-security/tools/install_demo_configuration.sh -y -i -s

##Perf Plugin
chmod 755 $OPENSEARCH_HOME/plugins/opensearch-performance-analyzer/pa_bin/performance-analyzer-agent
chmod 755 $OPENSEARCH_HOME/bin/performance-analyzer-agent-cli
echo ""done security""
PA_AGENT_JAVA_OPTS=""-Dlog4j.configurationFile=$OPENSEARCH_HOME/plugins/opensearch-performance-analyzer/pa_config/log4j2.xml \
			  -Xms64M -Xmx64M -XX:+UseSerialGC -XX:CICompilerCount=1 -XX:-TieredCompilation -XX:InitialCodeCacheSize=4096 \
			  -XX:InitialBootClassLoaderMetaspaceSize=30720 -XX:MaxRAM=400m""

OPENSEARCH_MAIN_CLASS=""org.opensearch.performanceanalyzer.PerformanceAnalyzerApp"" \
OPENSEARCH_ADDITIONAL_CLASSPATH_DIRECTORIES=plugins/opensearch-performance-analyzer \
OPENSEARCH_JAVA_OPTS=$PA_AGENT_JAVA_OPTS

if ! grep -q '## OpenSearch Performance Analyzer' $OPENSEARCH_HOME/config/jvm.options; then
   CLK_TCK=`/usr/bin/getconf CLK_TCK`
   echo >> $OPENSEARCH_HOME/config/jvm.options
   echo '## OpenSearch Performance Analyzer' >> $OPENSEARCH_HOME/config/jvm.options
   echo ""-Dclk.tck=$CLK_TCK"" >> $OPENSEARCH_HOME/config/jvm.options
   echo ""-Djdk.attach.allowAttachSelf=true"" >> $OPENSEARCH_HOME/config/jvm.options
   echo ""-Djava.security.policy=$OPENSEARCH_HOME/plugins/opensearch-performance-analyzer/pa_config/opensearch_security.policy"" >> $OPENSEARCH_HOME/config/jvm.options
fi
echo ""done plugins""

##Set KNN Dylib Path for macOS and *nix systems
if echo ""$OSTYPE"" | grep -qi ""darwin""; then
	if echo ""$JAVA_LIBRARY_PATH"" | grep -q ""$KNN_LIB_DIR""; then
		echo ""k-NN libraries found in JAVA_LIBRARY_PATH""
	else
		export JAVA_LIBRARY_PATH=$JAVA_LIBRARY_PATH:$KNN_LIB_DIR
		echo ""k-NN libraries not found in JAVA_LIBRARY_PATH. Updating path to: $JAVA_LIBRARY_PATH.""
	fi
else
	if echo ""$LD_LIBRARY_PATH"" | grep -q ""$KNN_LIB_DIR""; then
		echo ""k-NN libraries found in LD_LIBRARY_PATH""
	else
		export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$KNN_LIB_DIR
		echo ""k-NN libraries not found in LD_LIBRARY_PATH. Updating path to: $LD_LIBRARY_PATH.""
	fi
fi
";
			return
	//script from 2.0.0
	@"#!/bin/bash

# Copyright OpenSearch Contributors
# SPDX-License-Identifier: Apache-2.0

export OPENSEARCH_HOME=`dirname $(realpath $0)`
export OPENSEARCH_PATH_CONF=$OPENSEARCH_HOME/config
cd $OPENSEARCH_HOME

KNN_LIB_DIR=$OPENSEARCH_HOME/plugins/opensearch-knn/lib
##Security Plugin
bash $OPENSEARCH_HOME/plugins/opensearch-security/tools/install_demo_configuration.sh -y -i -s

echo ""done security""
PA_AGENT_JAVA_OPTS=""-Dlog4j.configurationFile=$OPENSEARCH_PATH_CONF/opensearch-performance-analyzer/log4j2.xml \
			  -Xms64M -Xmx64M -XX:+UseSerialGC -XX:CICompilerCount=1 -XX:-TieredCompilation -XX:InitialCodeCacheSize=4096 \
			  -XX:MaxRAM=400m""

OPENSEARCH_MAIN_CLASS=""org.opensearch.performanceanalyzer.PerformanceAnalyzerApp"" \
OPENSEARCH_ADDITIONAL_CLASSPATH_DIRECTORIES=plugins/opensearch-performance-analyzer \
OPENSEARCH_JAVA_OPTS=$PA_AGENT_JAVA_OPTS

if ! grep -q '## OpenSearch Performance Analyzer' $OPENSEARCH_PATH_CONF/jvm.options; then
   CLK_TCK=`/usr/bin/getconf CLK_TCK`
   echo >> $OPENSEARCH_PATH_CONF/jvm.options
   echo '## OpenSearch Performance Analyzer' >> $OPENSEARCH_PATH_CONF/jvm.options
   echo ""-Dclk.tck=$CLK_TCK"" >> $OPENSEARCH_PATH_CONF/jvm.options
   echo ""-Djdk.attach.allowAttachSelf=true"" >> $OPENSEARCH_PATH_CONF/jvm.options
   echo ""-Djava.security.policy=$OPENSEARCH_PATH_CONF/opensearch-performance-analyzer/opensearch_security.policy"" >> $OPENSEARCH_PATH_CONF/jvm.options
   echo ""--add-opens=jdk.attach/sun.tools.attach=ALL-UNNAMED"" >> $OPENSEARCH_PATH_CONF/jvm.options
fi
echo ""done plugins""

##Set KNN Dylib Path for macOS and *nix systems
if echo ""$OSTYPE"" | grep -qi ""darwin""; then
	if echo ""$JAVA_LIBRARY_PATH"" | grep -q ""$KNN_LIB_DIR""; then
		echo ""k-NN libraries found in JAVA_LIBRARY_PATH""
	else
		export JAVA_LIBRARY_PATH=$JAVA_LIBRARY_PATH:$KNN_LIB_DIR
		echo ""k-NN libraries not found in JAVA_LIBRARY_PATH. Updating path to: $JAVA_LIBRARY_PATH."" 
	fi
else
	if echo ""$LD_LIBRARY_PATH"" | grep -q ""$KNN_LIB_DIR""; then
		echo ""k-NN libraries found in LD_LIBRARY_PATH""
	else
		export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$KNN_LIB_DIR
		echo ""k-NN libraries not found in LD_LIBRARY_PATH. Updating path to: $LD_LIBRARY_PATH.""
	fi
fi
";
		}
	}
}
