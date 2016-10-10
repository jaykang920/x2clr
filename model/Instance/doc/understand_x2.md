# Unit Tests 

 - Binder, Buffer, Capo, Cell, Cipher, Config, Fingerprint, Handler, Hash, Pool, Sample, Serializer
 - They are not sufficient to fully cover 

## Guide 

### Refactoring Best / Worst

- [Material] (http://blog.stevensanderson.com/2009/08/24/writing-great-unit-tests-best-and-worst-practises/) 

#### Gist

 - Designing software components robustly
 - TDD is a design process, not a testing process
 - Changes to other parts of your codebase don’t make them(unit tests) start failing 


# HelloWorld in Examples

 - xpiler generates Event subclass from xml definition 
 - Event is a subclass of Cell 
 - Cell has a Tag and a Fingerprint 
    - Cell has several equal functions: 
        - Equals, EqualsTo
        - Equivalent(Cell), Equivalent(Cell, Finterprint)
    - Tag has Base tag, NumProps, 

## Fingerprint Unit Test 

 Fingerprint is a bit vector of fixed length. 

 block (0~31 bit) | blocks (higher than 32bit)

### Equivalent 

 Def. A is equivalent to B, if Fingerprint A covers and equals all bits of another Fingerprint B. 

### Deserialize

 It seems that desrialize has a bug or incomplete since: 
  - it does not set length and it does not allocate blocks.  

  


 

     