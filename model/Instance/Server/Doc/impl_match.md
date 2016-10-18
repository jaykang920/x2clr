# Match 


## Flow 

 - Client posts a Match request to session server 
 - LobbyCase on sessin server posts request to MatchCase. 
 - MatchCase search for proper Instance. 
   - If not found, then a proper Instance is created. 
     - Instance create request is posted InstanceCoordinator 
     - InstanceCoordinator selects an InstanceRunner and pass the request. 
     - InstanceRunner creates an Instance and respond to the MatchCase 
     - MatchCase marks the Instance with the requested user JoinPending. 
   - If found, then MatchCase marks the Instance with the requested user JoinPending 
    
 - MatchCase responds with MatchResult to the LobbyCase
 - LobbyCase responds with MatchResult 
 - Then, Client requests Join to the LobbyCase to the Instance. 
   - Instance reports instance status to the MatchCase 
 
## Rank 

 - Rank is added to Match and Instance. 
 - MatchCase binds to Rank value
  
## Events 

 Start with detailed events and consider to add base event class to handle more efficiently.

### Match Events 

 EventInstanceMatchReq 

 EventInstanceMatchResp 
 
### Instance Events 

 EventInstanceCreateReq 

 EventInstanceCreateResp 

 EventInstanceJoinReq

 EventInstanceJoinResp

 EventInstanceLeaveReq 

 EventInstanceLeaveResp 

 EventInstanceDestroyed

