Feature: Validate Navigation to Services Section
As a anonymous visitor
I want to review Services

@tag1
Scenario Outline: Navigate to Services by heading
   Given I am on Main Page
   When I hover over Services
   And I click on "<heading>"
   Then I see the same "<heading>"
   And there is 'Our Related Expertise' section

   Examples:
     | heading |
     | Generative AI |
     | Responsible AI |
