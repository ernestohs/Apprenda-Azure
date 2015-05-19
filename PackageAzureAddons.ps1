# Azure Scripts Packaging
# Author: Chris Dutra
# Contact: cdutra@apprenda.com

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
    echo "ModuleName : $moduleName"
    echo "ModulePath : $modulePath"
    echo "Build Directory: $buildDir"

    if(Test-Path $buildDir)
    {
        echo "Test path for $buildDir works."
        # if exists, back it up.
        $date = Get-Date -UFormat "%Y%m%d%H%M%S"
        #mv $buildDir "$currentPath\Archive\$moduleName.$date"
    }
    #mkdir $buildDir
    # recurive traversal to find bin folder.
    $bin = Get-ChildItem $modulePath -Recurse | ?{$_.Name.Equals("bin")}
    $binPath = $bin.FullName

    echo $binPath
    #cp -Recurse $binPath\* $buildDir

    # just add them 1 by 1
    #Zip-Directory -DestinationFileName "$currentPath\$modulePath.latest.zip" -SourceDirectory $buildDir
}

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
