
# Channel / Bind guide

 Distribution needs to be considered.
 Partition of sent events needs to be considered. 

 Channels: 
 
 - Server processing Channels 
   - Ex. Login, Lobby, InstanceRunner1, InstanceRunner2, Cluster, etc. 
 - Net send Channels 
   - One for all accepted sessions. Usually ServerName + Net 
     - Ex. SessionNet, MasterNet, GameNet 
   - One for each AsyncTcpClient to other servers. 
     - Ex. MasterClient, GameClient1, GameClient2, etc 
     
# Demonstraction     

 Demo shows Login / logout processing can be setup for local and distributed channel setup.
 This can help building application with unit tests, then finish with distributed configuration.
 
## Classes 

 <> has 
    - <receiving channels, posting channels> 
 
  
## Local 
 
 - LoginCase : <Session | SessionCient | MasterNet, SessionNet | MasterClient> 
 - DirectoryUser : <Master | MasterClient, MasterNet>  
 - ClientCase : <SessionNet, SessionClient>
     

Login flow: 

 - ClientCase posts EventLoginReq to SessionClient channel
 - LoginCase posts EventLoginReq to MasterClient channel 
 - DirectoryUser posts EventLoginResp to MasterNet channel 
 - AuthCase Posts EventLoginResp to SessionNet channel 

## Distributed 

Copied from local: 

 - LoginCase : <Session, MasterClient | SessionNet> 
 - DirectoryUser : <User, MasterNet>  
 - ClientCase : <SessionNet, SessionClient>

 - SessionNet : <SessionNet, _> 
   - Preprocess sets Event._Channel to Login (Depending on TypeId)  
 - MasterClient : <MasterClient, _> 
   - Preprocess sets Event._Channel to Login (Depending on TypeId)
 - MasterNet : <MaserNet, _> 
   - Prepress sets Event._Channel to User (Depending on TypeId)
   
 Login flow: 

 - ClientCase posts EventLoginReq to SessionClient channel
   - SessionClient sends to a session of SessionNet 
   - SessionNet sets Event._Channel to Login 
 - LoginCase posts EventLoginReq to MasterClient channel 
   - MasterClient sends to a session of MasterNet
   - MasterNet sets Event._Channel to Login
 - DirectoryUser posts EventLoginResp to MasterNet channel 
   - MasterNet sends to a session of SessionNet
   
# Summary 

 The above process can be generalized. 
 
    