#! /bin/bash

BUILDVERSION=${1:-./BuildVersion/BuildVersion.exe}

$BUILDVERSION \
        --verbose \
        --namespace org.herbal3d.cs.CommonUtil \
        --version $(cat VERSION) \
        --assemblyInfoFile Properties/AssemblyInfo.cs
