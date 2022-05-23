**********************************
SolutionConverter application 
**********************************
converts a solution that represents an Automation script to XML.

Expects following arguments

args[0] = path to sln file. 
args[1] = destinationPath. Require folder name to be "__SLC_CONVERTED__"
args[2] = disFilesPath
args[3] = "automation" (type)
e.g.

 .\SolutionConverter.exe "C:\GIT\Automation\Internal\Skyline\Automation Certification\AutomationScript.sln" "C:\TestEnv\__SLC_CONVERTED__" "C:\TestEnv\DisFiles" "automation"

************************************
DmAppPackageCreator application
************************************
Expects following arguments

args[0] = workspace 
args[1] = jobBaseName, obtain from gitHub
args[2] = tagName, obtain from gitHub
args[3] = branchName, obtain from gitHub. In case no branch, use " ".
args[4] = buildNumber, obtain this from gitHub
args[5] = type, needs to be "Automation"
args[6] = Origin , needs to be "Customer";
args[7] = Version, format is x.x.x
args[8] = installScriptFilePath. optional. in case of empty, the install file located in \InstallScript\Install.xml will be used.

e.g.

.\DmAppPackageCreator.exe "C:\TestEnv" "Demo Driver" "tag" "testBranch" "1" "Automation" "Customer" "1.0.1" "C:\TestEnv\Install.xml"

**************************************
DisFiles
**************************************

DIS files which are being used by SolutionConverter.