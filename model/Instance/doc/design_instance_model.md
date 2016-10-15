# Session 

## Classes

 - SessionServer
   - Is a AsyncTcpServer
   - Has Users
   - Handles Disconnect / Connect 
   
 - LoginCase 
   - Handles Login, Logout 
  
 - LobbyCase 
   - Handles match / join / leave instance 
   
 - GameServerClient 
   - Is a AsyncTcpClient 
   - Connects to a GameServer
   - Has a list in SessionServer 
   
## Channel 

 - AsyncTcpClient is in one SingleThreadFlow with a Channel set with the server name or id
   - Example, "master", "gs1", "gs2", ... 
 - AsyncTcpServer is in one SingleThreadFlow with "Clients" Channel  
 - SingleThreadFlow w/ Login Channel for LoginCase 
 - SingleThreadFlow w/ Lobby Channel for LobbyCase
 
 
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
   - Can run in any flow (Strictly Actor)
   
## Channel 

 - "Clients" for all accepted servers
 - "App" for Instance, Server, User directory regardless of Flow
 - Multicasting to Flow queues happens, but keep x2 scheme and enhance performance later with profiling.
    
# Game 

 - GameServer 
   - Is a AsyncTcpServer 
   
 - InstanceRunner
   - Is a Case
   - Creates and manages Instances
   - Runs in a SingleThreadFlow
   
 - InstanceFactory 
   - A singleton 
   - Instance Type based factory
   
 - Instance 
   - A class 
   - Callback through Bind
   

   
   
    