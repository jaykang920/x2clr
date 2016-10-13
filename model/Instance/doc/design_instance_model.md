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
   
## Runtime Configuration 

 - A SingleThreadFlow subscribed to "Net"
   - Has SessionServer
   - Preprocess changes _Channel to "App"
   
 - A SingleThreadFlow subscribed to "Cluster" 
   - Has Clients to Servers  
   - Preprocess changes _Channel to "App" 
   
 - A SingleThreadFlow subscribed to "App"
   - Has LoginCase and LobbyCase 
   - Change _Channel to "Net" when sending to clients 
   - Change _Channel to "Cluster" when sending to servers 

 - The above setup seems not natural. There must be a better way. 
   - Because 
     - Only Flow can subscribe to Channel.
     - Flow is not a natural way to partition queue processing. 
      
   
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
   
   
   
    