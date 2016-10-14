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
   
   
   

     
  
  