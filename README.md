# Pipeline
Internal middleware pipeline system with: fluent-api step composition, immutable context delivery, async execution optionality, explicit before/short-circuit/after resolving designed to keep gameplay logic modular, testible, and easy to evolve

## 💻 Technologies Used

1. C#
2. Unity3D

## 💎 Features

1. Construct middleware pipelines via fluent-api builder
2. Order and re-order pipeline steps in a modular way
3. Type-safe, immutable context passing that acts as a snapshot avoiding mid-pipeline runtime discrepancies
4. Short-Circuit avaliable steps that terminate pipeline execution based on custom predicates
5. Resolver integration for each and every step: before, after, and during a potential short-circuit
6. Resolver avaliable functionality increasing modularity and extensibility
7. Async specific resolve options during pipeline execution (Waiting, TimedGates)

## Example Usage

### Example 1 — Smallest possible pipeline (just do one thing)
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

### Example 2 — Add validation with `Add_ShortCircuit`
If a short-circuit condition fails, the main method won’t run.

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

### Example 3 — Add `before`, `after`, and `shortCircuited` hooks
This is great for logging, VFX/SFX, analytics, or UI feedback.

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

### Example 4 — Multi-step gameplay flow (realistic)
This combines multiple guards and a final action.

```csharp
using EMILtools.Core;
using EMILtools.Systems;

public sealed class FireCtx : IContextViewImmutable
{
    public bool HasAmmo;
    public bool IsReloading;
    public bool IsAlive;
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
    .InjectMainMethod(ctx =>
    {
        UnityEngine.Debug.Log("Bang!");
        // consume ammo, spawn projectile, play VFX/SFX, etc.
    });

await PipelineExecutor<FireCtx>.TryTo(firePipeline, new FireCtx
{
    HasAmmo = true,
    IsReloading = false,
    IsAlive = true
});
```

## 🔀 Flowchart

![Pipeline Flowchart](https://github.com/user-attachments/assets/8a156396-2fd6-4606-99e0-f6d5b3301870)
