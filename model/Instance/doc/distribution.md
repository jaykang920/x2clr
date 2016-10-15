# Problem Assessment 

 - x2 basically broadcasts Events to all Flows through a singleton Hub
   - This makes it very easy to propagate events to required Flows and Cases. 
   - This makes locational transparency possbile through AsyncTcpServer Case binding to Events. 
   
 - The above brings very powerful communication mechanism: 
   - Bind and Post only scheme.  (No Send call)
     - Locational transparency
   - Easy multicasting 

 - But broadcasting / flow queue based dispatching brings performance issue. 
   - Typical setup is to have a SingleThreadFlow for a AsyncTcpServer. 
   - When a Case Posts event, it is passed to all Flows, then to the AsyncTcpServer
   - The AsyncTcpServer binds to Send, then the Event is passed to the remote. 
   - Performance issue happens since:  
     - the SingleThreadFlow queue is used for received and posted events at the same time
     - the message filtering only works with _Channel / Channel subscribed to Hub
     - Posted events need to wait received event processing till it can be posted to remote. 
     - using Preprocess to set Channel for received events can solve the queueing delay issue. 

## Possible Solutions 

 - Have a Channel dedicated to each Flow which has AsyncTcpServer or AsyncTcpClient       
   - This can be an easy and instant solution. 
   - When cluster topology has lots of AsyncTcpClient to many other servers
     - Channel setup is possible with SingleThreadFlow 
     - SingleThreadFlow blocks waiting mostly
     - Still queueing delay happens competing with received events. 
     
 - One solution added is to have Post() with direct send      
   - Override Flow.Feed() in subclass of SingleThreadFlow to send to the _Handle when: 
     - The Flow has AsyncTcpClient or AsyncTcpServer 
     - The Event _Handle != 0   
   - This can solve the queueing delay issue keeping x2 spirit as a specialization for distributed communication. 
   
 - NetFlow added. 
   - Some of the x2 semantics seem to be broken. 
   - Cleanup and match as much as possible to x2 scheme. 
   
## Feedback from Author 

 - Bind / Post with Flow is suggested.
 - Channel based Flow filtering is suggested. 
 
 - The above setting can decrease queueing delay for remote messages 
 - Setting Channel for received Events is still an issue.   
 
## Oh! Good 

 - Instead of using Preprocess to set Channel, set Channel when Post. 
   - Make a consistent scheme for filtering Flows with Channel name. 
 - Make Flows subscribe for all Channels required. 
   - Flow can subscribe for multiple Channels. 
   
 Therfore, designing Channels is the core of x2 design. 

# Conceptualization

Core concepts in x2: 

- Flow is a processing core having Cases as logic unit
- Type / Value based Event Bind / Post
- Flow Channel filtering for optimization
  - Upstream : From network to application
  - Downstream : From application to network
  
Setup with the above concepts can have good distributed server. 

I will prove it with Instance model by example.

