# Session 

## Role 

 - Manages sessions from Clients 
 - Login / Lobby functionality for Clients 
 - Relay Game events to and from Clients 
 
## Classes 

 - LoginCase 
 - LobbyCase 
 - SessionServer 
 - MasterClient 
   - Works in a separated flow "MasterClient"
 - GameClient 
   - Works in a separated flow "GameClient{0}" % Id 

## Construction w/ Tests

### Channel / Bind guide

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

 
### TestLogin

Local test wo/ *Net

  - 3 flows     
    - Client / SessionNet running ClientCase
    - Master / MasterClient running DirectoryUser Case
    - MasterNet / Session running LoginCase 

  - ClientCase Post with Session Channel 
  - Master Cases Post with MasterNet Channel 
  - Session Cases Post with Master or Client depending on Events 

Distributed

  - 3 flows 
    - Client running SessionClient / ClientCase 
    - Master / MasterNet running MasterNet and DirectoryUser Case 
    - Session / MasterClient / SessionNet running SessionNet, LoginCase and MasterClient
    
The difference is *Net class involved or not.
Channel setting can be same for both cases. 

### TestLobby


 