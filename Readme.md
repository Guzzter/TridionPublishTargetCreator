TridionPublishTargetCreator 
------------------------
- Uses config-less code from http://code.google.com/p/tridion-practice/wiki/GetCoreServiceClientWithoutConfigFile
- Built using Tridion 2011 SP1
- Need to add the Tridion DLL Reference.  The Tridion DLL Tridion.ContentManager.CoreService.Client is located in the Tridion\bin\client folder.
- Example CVS file is stored in CsvFiles\server_environment.config
- CSV contains a header with the column names: name,target,targettype,linked pubs
- Linked pubs are resolved by search for publication with a certain prefix
- Tip: create a file per DTAP environment