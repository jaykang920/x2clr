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

## Construction 

 x2 can change cluster configuration easily.

 Therefore, construction goes in following order: 

 - functional tests with NUnit
 - setup Cases 
 - setup Event Bind
 - setup ChannelFilter
 

## DirectoryServer 


 
 
  