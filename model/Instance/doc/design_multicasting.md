# x2 way of doing multicasting

 - Multicasting is required on each server. 
 - x2 approach is to create an EventSink instance on session connected. 
 - That session binds to application event with value. 
 - That session can send to the session. 

The configuration change should not affect bindings. 
That's the x2 goal. 

# Process 

 - Develop cases and tests 
 - Make distributed configuration 
   - Setup distribution bindings on EventSink subclass 
   - Test and confirm

  


 