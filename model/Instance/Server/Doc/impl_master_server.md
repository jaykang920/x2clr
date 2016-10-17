# Master

## Classes 

 - MasterServer
   - Is a AsyncTcpServer
   
 - DirectoryServer
   - Server directory 
   - Maintains status 
   - Passes status to all other servers 
   
 - DirectoryInstance 
   - Is a Case 
   - Handles Match 
   - Handles Instance creation / destroy
   - Has a Type Indexer 
   - Can exist serveral directories
      
 - DirectoryUser 
   - Is a Case 
   - Handles Login / Logout / Location change 
   - Has a Rank Indexer 
   
 - Common to directories
   - Can run in any flow (Strictly Actor)
   
## DirectoryServer 

 - Status synchronization 
 - ServerList has a list of Servers 
 - Role is required 
 - TestDirectoryServer shows Join / ServerList flow 
 
 Next implementation is SessionServer / Client, 
 then comes back to DirectoryUser for authentication. 
  
## MasterServer 

 
  