# Pipeline
Internal middleware pipeline system with: fluent-api step composition, immutable context delivery, async execution optionality, explicit before/short-circuit/after resolving designed to keep gameplay logic modular, testible, and easy to evolve

## 💻 Technologies Used

1. C#
2. Unity3D

## 💎 Features

1. Construct middleware pipelines via fluent-api builder
2. Order and re-order pipeline steps in a modular way
3. Type-safe, immutable context passing that acts as a snapshot avoiding mid-pipeline runtime descrprensise
4. Short-Circuit avaliable steps that terminate pipeline execution based on custom predicates
5. Resolver integration for each and every step: before, after, and during a potential short-circuit
6. Resolver avaliable functionality increasing modularity and extensibility
7. Async specific resolve options during pipeline execution (Waiting, TimedGates)

## Example Usage



## 🔀 Flowchart

![Pipeline Flowchart](https://github.com/user-attachments/assets/8a156396-2fd6-4606-99e0-f6d5b3301870)
