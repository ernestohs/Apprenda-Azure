# Azure Scripts Packaging
# Author: Chris Dutra
# Contact: cdutra@apprenda.com

function Zip-Directory {
    Param(
      [Parameter(Mandatory=$True)][string]$DestinationFileName,
      [Parameter(Mandatory=$True)][string]$SourceDirectory,
      [Parameter(Mandatory=$False)][string]$CompressionLevel = "Optimal",
      [Parameter(Mandatory=$False)][switch]$IncludeParentDir
    )
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $CompressionLevel    = [System.IO.Compression.CompressionLevel]::$CompressionLevel  
    [System.IO.Compression.ZipFile]::CreateFromDirectory($SourceDirectory, $DestinationFileName, $CompressionLevel, $IncludeParentDir)
}

# Just to be safe...
Set-ExecutionPolicy Unrestricted -Scope Process
# get the path of this file, no matter where its run
cd $PSScriptRoot
$modules = Get-ChildItem .\Apprenda-Azure-Addon | ?{$_.PSIsContainer} | select Name,FullName
# for each Azure module, traverse and copy bin folders
foreach($child in $modules)
{
    if($child.FullName.Contains('packages'))
    {
        continue
    }
    $moduleName = $child.Name
    $modulePath = $child.FullName
    $buildDir = "$PSScriptRoot\Apprenda-Azure-Build\$moduleName"
    $archiveDir = "$PSScriptRoot\Apprenda-Azure-Build\Archive"
    echo "ModuleName : $moduleName"
    echo "ModulePath : $modulePath"
    echo "Build Directory: $buildDir"

    if(Test-Path $buildDir)
    {
        echo "Test path for $buildDir works."
        # if exists, back it up.
        $date = Get-Date -UFormat "%Y%m%d%H%M%S"
        # if the archive directory isn't set up, then create it
        if(!(Test-Path $archiveDir)) { mkdir $archiveDir }
        Zip-Directory -DestinationFileName "$archiveDir\$moduleName.Debug.$date.zip" -SourceDirectory $buildDir\Debug 
        Zip-Directory -DestinationFileName "$archiveDir\$moduleName.Release.$date.zip" -SourceDirectory $buildDir\Release 
        rm -Recurse $buildDir
        rm "$PSScriptRoot\$moduleName.Debug.latest.zip"
        rm "$PSScriptRoot\$moduleName.Release.latest.zip"
    }
    
    mkdir $buildDir
    # recurive traversal to find bin folder.
    $bin = Get-ChildItem $modulePath -Recurse | ?{$_.Name.Equals("bin")}
    $binPath = $bin.FullName

    # package up both debug and release
    mkdir $buildDir\Debug
    mkdir $buildDir\Release
    cp -Recurse $binPath\Debug\* $buildDir\Debug
    cp -Recurse $binPath\Release\* $buildDir\Release

    # just add them 1 by 1
    Zip-Directory -DestinationFileName "$PSScriptRoot\$moduleName.Debug.latest.zip" -SourceDirectory $buildDir\Debug
    Zip-Directory -DestinationFileName "$PSScriptRoot\$moduleName.Release.latest.zip" -SourceDirectory $buildDir\Release
}

