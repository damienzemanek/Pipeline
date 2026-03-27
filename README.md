# Pipeline
Internal middleware pipeline system with: fluent-api step composition, immutable context delivery, async execution optionality, explicit before/short-circuit/after resolving designed to keep gameplay logic modular, testable, and easy to evolve

## 💻 Technologies Used

- C#
- Unity3D


## 💎 Features

1. Construct middleware pipelines via fluent-api builder
2. Order and re-order pipeline steps in a modular way
3. Type-safe, immutable context passing that acts as a snapshot avoiding mid-pipeline runtime discrepancies
4. Short-Circuit avaliable steps that terminate pipeline execution based on custom predicates
5. Resolver integration for each and every step: before, after, and during a potential short-circuit
6. Resolver avaliable functionality increasing modularity and extensibility
7. Async specific resolve options during pipeline execution (Waiting, TimedGates)



## 🎮 Example Usage

### 📌 Example 1 — Smallest possible pipeline 
Declaring a pipeline is easy

```csharp
using EMILtools.Systems;

public sealed class MoveCtx : IContextViewImmutable
{
    public float Speed;
}

var movePipeline = new PipelineBuilder<MoveCtx>()
    .InjectMainMethod(ctx =>
    {
        // Main action only
        UnityEngine.Debug.Log($"Moving at speed: {ctx.Speed}");
    });

await PipelineExecutor<MoveCtx>.TryTo(movePipeline, new MoveCtx { Speed = 5f });
```


### 📌 Example 2 — Add middleware functionality with `Add_Middleware`
Compose pipelines with extra functionality separate from eachother, (ex: animation, vfx, preparation)

```csharp
using EMILtools.Systems;

public sealed class StaminaCtx : IContextViewImmutable
{
    public int Stamina;
}

var sprintPipeline = new PipelineBuilder<StaminaCtx>()
    .Add_Middleware(ctx =>
    {
        // Middleware runs before validation/main method
        UnityEngine.Debug.Log($"Preparing sprint with stamina: {ctx.Stamina}");
    })
    .InjectMainMethod(ctx =>
    {
        UnityEngine.Debug.Log("Sprint executed.");
    });

await PipelineExecutor<StaminaCtx>.TryTo(sprintPipeline, new StaminaCtx { Stamina = 20 });
```


### 📌 Example 3 — Add validation with `Add_ShortCircuit`
If a short-circuit condition fails, the main method won’t run. (or all steps after short-circuiting)

```csharp
using EMILtools.Core;
using EMILtools.Systems;

public sealed class AttackCtx : IContextViewImmutable
{
    public bool HasWeapon;
}

var attackPipeline = new PipelineBuilder<AttackCtx>()
    .Add_ShortCircuit(new FuncCtxPredicate<AttackCtx>(ctx => ctx.HasWeapon))
    .InjectMainMethod(ctx =>
    {
        UnityEngine.Debug.Log("Attack executed.");
    });

await PipelineExecutor<AttackCtx>.TryTo(attackPipeline, new AttackCtx { HasWeapon = true });  // runs
await PipelineExecutor<AttackCtx>.TryTo(attackPipeline, new AttackCtx { HasWeapon = false }); // blocked
```


### 📌 Example 4 — Add `before`, `after`, and `shortCircuited` hooks
Add dynamic branching logic to individual steps depending on what happens. (each resolve is optional via " = null")

```csharp
using EMILtools.Core;
using EMILtools.Systems;

public sealed class CastCtx : IContextViewImmutable
{
    public int Mana;
}

var castPipeline = new PipelineBuilder<CastCtx>()
    .Add_ShortCircuit(
        new FuncCtxPredicate<CastCtx>(ctx => ctx.Mana >= 10),
        before: new IResolvable[]
        {
            new Callback(() => UnityEngine.Debug.Log("Checking mana..."))
        },
        after: new IResolvable[]
        {
            new Callback(() => UnityEngine.Debug.Log("Mana check passed."))
        },
        shortCircuited: new IResolvable[]
        {
            new Callback(() => UnityEngine.Debug.Log("Not enough mana."))
        })
    .InjectMainMethod(ctx =>
    {
        UnityEngine.Debug.Log("Spell cast!");
    });

await PipelineExecutor<CastCtx>.TryTo(castPipeline, new CastCtx { Mana = 15 }); // before -> after -> main
await PipelineExecutor<CastCtx>.TryTo(castPipeline, new CastCtx { Mana = 3 });  // before -> shortCircuited
```


### 📌 Example 5 — Multi-step gameplay flow (realistic)
This combines multiple guards and a middleware with the final main execution.

```csharp
using EMILtools.Core;
using EMILtools.Systems;

public sealed class FireCtx : IContextViewImmutable
{
    public bool HasAmmo;
    public bool IsReloading;
    public bool IsAlive;
    public int Recoil;
}

var firePipeline = new PipelineBuilder<FireCtx>()
    .Add_ShortCircuit(new FuncCtxPredicate<FireCtx>(ctx => ctx.IsAlive))
    .Add_ShortCircuit(new FuncCtxPredicate<FireCtx>(ctx => !ctx.IsReloading))
    .Add_ShortCircuit(
        new FuncCtxPredicate<FireCtx>(ctx => ctx.HasAmmo),
        shortCircuited: new IResolvable[]
        {
            new Callback(() => UnityEngine.Debug.Log("Click! Out of ammo."))
        })
    .Add_Middleware(ctx =>
    {
        // Middleware step before main fire action
        ctx.Recoil += 1;
        UnityEngine.Debug.Log($"Applying recoil: {ctx.Recoil}");
    })
    .InjectMainMethod(ctx =>
    {
        UnityEngine.Debug.Log("Bang!");
        // consume ammo, spawn projectile, play VFX/SFX, etc.
    });

await PipelineExecutor<FireCtx>.TryTo(firePipeline, new FireCtx
{
    HasAmmo = true,
    IsReloading = false,
    IsAlive = true,
    Recoil = 0
});
```

## 📐 My Development Process

1. I was iterating on another system of mine and wanted to find a way to separate out the transient state context from the actual blackboard I was using. So I started brainstorming using Figma
2. I came up with an initial design alongside developing an initial integration flowchart of how it would fit in my other system
3. I initially thought that using value-types would be a viable solution, however I quickly transitioned to reference-types when I realized how often the system sends the context around.
If it were a value-type it would be copied by value many many times. In turn, I realized a design constraint I needed to keep in mind, that being: High throughput
5. I then started prototyping in Unity on how the system would function independantly 
6. After A couple iterations on the programming and updates to my diagrams I got it to a usable state.
7. I then used Unity's Test Runner and tested my initial iteration of the Pipelines system pass using NUnit and UnityTests.
8. After successfully passing all NUnit and UnityTests I implemented and integrated it into my other system
9. Furthermore, In developing other systems I've updated my Pipelines system with Resolve functionality and Predicate abstraction to further SRP-out Pipelines

You can take a look at my Diagram iterations [here](https://drive.google.com/drive/folders/1l6R2zXRntVr-AAUXhpVh-s5f02nbHClp?usp=sharing)


## 💡 What I learned

### 🔧 Context Immutability Design
- There are different levels to immutability in different programming layers to make data immutable. The one I chose for this project was to make the Context be immutable at the class level.
This means instead of using { get; private set; } which still allows class references to be mutated because the private set only affects the the immutability at the replacement level, I would implement
an interface with { get; } only and separate out any class references from the concrete context class itself. So what it looks like is this:
```csharp

public interface ISomeContextView
{
    public float someFloat { get; }
}

public sealed class SomeContextData : ContextData, ISomeContextView
{
    public float someFloat { get; private set; } 
}

```
The neat thing about this is that the implementation of the ContextView interface is integrated within the { get; private set; } definition of the concrete class. Meaning there are no extra semantic indirection
caused by other things like protected interface implementations.

### 🔨 Performance Considerations for Data Throughput
- Another important peice of information I gleaned is that high-throughput is important for systems that constantly send data around. Meaning using value-types even if the data is small, is not a good design descision
based on the rate at which data is sent. The pipeline system is designed for the Unity Player Loop, meaning it can hook into Update, FixedUpdate, etc. Therefore the execution can be called every frame. This would be a
problem with a value-type system. However using reference-types the issue of rapid copies creating many one-shot copies is avoided.


# 🔀 Flowchart

![Pipeline Flowchart](https://github.com/user-attachments/assets/8a156396-2fd6-4606-99e0-f6d5b3301870)
