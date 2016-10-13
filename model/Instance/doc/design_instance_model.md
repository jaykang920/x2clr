# Session 

 - SessionServer
   - Is a AsyncTcpServer
   - Has Users
   - Handles Disconnect / Connect 
   
 - LoginCase 
   - Handles Login, Logout 
  
 - LobbyCase 
   - Handles match / join / leave instance 
   
# Master 

 - MasterServer
   - Is a AsyncTcpServer
   
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
   - Can run any flow (Strictly Actor)
    
# Game 

 - GameServer 
   - Is a AsyncTcpServer 
   
 - InstanceRunner 
   - Is a SingleThreadFlow with a Channel name assigned. 
   
 - InstanceDirector 
   - Is a Case that dispatch Events to Instances
   
    