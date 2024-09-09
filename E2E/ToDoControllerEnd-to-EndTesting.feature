Feature: ToDo Controller End-to-End Testing

  Background:
    * url 'http://localhost:35000/to-dos-api'

  Scenario: Verify CRUD operations on ToDo items
  
    Given path 'to-dos'
    When method get
    Then status 200
    And match response contains { toDos: '#[]' }

    Given path 'to-dos'
    * def randomName = 'Test ' + java.util.UUID.randomUUID().toString()
    And request { name: '#(randomName)' }
    When method post
    Then status 200
    * def addedToDoId = response
    
    Given path 'to-dos'
    When method get
    Then status 200
    And match response.toDos[*].name contains randomName
    
    Given path 'to-dos/complete'
    Given request { todoIds: [#(addedToDoId)] }
    When method post
    Then status 200
    
    Given path 'to-dos'
    When method get
    Then status 200
    And def responseContainsTest = karate.jsonPath(response, "$.toDos[?(@.name == 'Test')]").length > 0
    And def responseContainsRandomName = karate.jsonPath(response, "$.toDos[?(@.name == '#(randomName)')]").length > 0
    And match responseContainsTest == false && responseContainsRandomName == false
