
# Channel / Bind guide

 Distribution needs to be considered.
 Partition of sent events needs to be considered. 

 Channels: 
 
 - Server processing Channels 
   - Ex. Login, Lobby, InstanceRunner1, InstanceRunner2, Cluster, etc. 
 - NetServer send Channels 
   - One for all accepted sessions. Usually ServerName + NetServer 
     - Ex. SessionNetServer, MasterNetServer, GameNetServer 
   - One for each AsyncTcpNetClient to other servers. 
     - Ex. MasterNetClient, GameNetClient1, GameNetClient2, etc 
     
# Demonstraction     

 Demo shows Login / logout processing can be setup for local and distributed channel setup.
 This can help building application with unit tests, then finish with distributed configuration.
 
## Classes 

 {} has 
    - {receiving channels, posting channels} 
 
  
## Local 
 
 - LoginCase : {Session | SessionCient | MasterNetServer, SessionNetServer | MasterNetClient} 
 - DirectoryUser : {Master | MasterNetClient, MasterNetServer}  
 - ClientCase : {SessionNetServer, SessionNetClient}

Login flow: 

 - ClientCase posts EventLoginReq to SessionNetClient channel
 - LoginCase posts EventLoginReq to MasterNetClient channel 
 - DirectoryUser posts EventLoginResp to MasterNetServer channel 
 - AuthCase Posts EventLoginResp to SessionNetServer channel 

## Distributed 

Copied from local: 

 - LoginCase : {Session, MasterNetClient | SessionNetServer} 
 - DirectoryUser : {User, MasterNetServer}  
 - ClientCase : {SessionNetServer, SessionNetClient}

Network middlemen: 

 - SessionNetServer : {SessionNetServer, _} 
   - Preprocess sets Event._Channel to Login (Depending on TypeId)  
 - MasterNetClient : {MasterNetClient, _} 
   - Preprocess sets Event._Channel to Login (Depending on TypeId)
 - MasterNetServer : {MaserNetServer, _} 
   - Prepress sets Event._Channel to User (Depending on TypeId)
   
Login flow: 

 - ClientCase posts EventLoginReq to SessionNetClient channel
   - SessionNetClient sends to a session of SessionNetServer 
   - SessionNetServer sets Event._Channel to Login 
 - LoginCase posts EventLoginReq to MasterNetClient channel 
   - MasterNetClient sends to a session of MasterNetServer
   - MasterNetServer sets Event._Channel to Login
 - DirectoryUser posts EventLoginResp to MasterNetServer channel 
   - MasterNetServer sends to a session of SessionNetServer
   
# Summary 

 Setting up Channels properly with Preprocess 
 can coordinate Local / Distributed processing transparently. 
 
  
    