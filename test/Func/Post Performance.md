# Problem Assessment

 - x2 has Flow broadcasting scheme. 
 - Very efficient for dev. and reconfiguration
 - Performance is important to feel comfortable with this scheme.
 
# Measurement

 - 1 SingleThreadFlow  
   - Shows 1 million Post / Bind processing in 1 second. 
   - Very fast. 

 - 4 SingleThreadFlow. Only one flow processing Bind / Post. 
   - Shows 1 million Post / Bind processing in 4 seconds. 

 - 8 SingleThreadFlow. Only one flow processing Bind / Post. 
   - Shows 1 million Post / Bind processing in 8 seconds. 
   
 - Filtering with Event._Channel doesn't help. 
   - This shows that thread scalability is an issue.  

# Optimization 

 - Broadcasting is not an issue. 
 - Contrary to the intuition, threads contention is an issue. 
 
## Thread Contention

 - Monitor.Wait / Monitor.Pulse does not scale well with multiple threads. 
 - ConcurrentThreadFlow is added to use ConcurrentQueue removing Monitor dependency. 
 - x2clr_s is added to change .NET platform to 4.5.2 to support ConcurrentQueue 
 
## Result 

 - Scales very well 
 - 1 million events per second 
   - broadcasted to 4 flows 
   - handled on a single flow.
   
# Summary 

 With this optimization for server side processing, x2 scheme can be kept with performance. 
 
 

 