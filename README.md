# LifeSimulation
Simulation foodchain behaviour for comparing different languages

For comparing different languages I decide to take task from book "AI Application Programming" M. Tim Jones, Charles river media, inc, 2003, Chapter 7 "Artifial life"

## Comparing task
Modeling foodchain

Carnivore → Herbivore → Plant

### Rules
* Cycled rectangled field is used that is if you take a step through bound of the field you be moved to oposite bound of the field.
* Plants is fixed object of food.
* Herbivores is moving agents that eat plants.
* Carnivores is moving agents that eat herbivores.
* Carnivores and herbivores perceive environment in its own way
* Carnivores and herbivores make decision of its action by neural network (one layer perceptron)
* If agent doesn't eat a long time, it dies
* When agent eat much enough, it can reproduce oneself, thus new agent is created in the environment.
* Each new agent has its own (random initialized) neural network that is its own new behaviour. It makes evolution process.

### Agent behaviour
An areas of agent perception is defined as shown in follow table 

![Sensors table](https://github.com/xilec/LifeSimulation/blob/master/images/SensorsTable.png)

Output signals formula

![Output signals formula](https://github.com/xilec/LifeSimulation/blob/master/images/OutputSignalsFormula.png)

where
  o - output signals
  b - parameters of perceptron which independent from input signals
  u - input signals
  w - weight of neural network

Inputs of 1-layer perseptron are defined by next table

 index | agent | area
 ------|-------|-----
  0 | herbivore | front
  1 | carnivore | front
  2 | plant | front
  3 | herbivore | left
  4 | carnivore | left
  5 | plant | left
  6 | herbivore | right
  7 | carnivore | right
  8 | plant | right
  9 | herbivore | proximity
  10 | carnivore | proximity
  11 | plant | proximity
 
 Outputs of 1-layer perseptron are defined by next table
 
 index | action
 ------|-------
 0 | turn left
 1 | turn right
 2 | move to the front
 3 | eat
 
 Executed action are selected by rule "winner take all"
 
### Life and death
 * Each agent has its own level of energy which is decreased every turn and it is increased after eating
 * When energy level more then particular threshold, agent can reproduces a child
 * When energy level achive a zero, agent dies