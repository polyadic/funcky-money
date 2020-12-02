# funcky-money

Funcky.Money is an implementation of a versatile Money concept which addresses a lot of the problems you encounter when working with money quantities and currencies.

The implementation uses a lot of the concepts from the TDD implementation of Kent Beck in his book: Test-Driven Development by Example.

## How can Money be Funcky?

The Money approach is distinctly functional especially in comparison to the Money Pattern by Martin Fowler which just gives up when it comes to currencies.

The basic idea is, instead of trying to calculate the final amount in one step, we start with an Expression Tree (AST: Abstract Syntax Tree) of the operations we want to perform with Money objects.

This allows us to decouple the assembly and the evaluation of our calculations, our calculation are lazy and allow deferred execution.


## Why not a [money bag](https://deque.blog/2017/08/17/a-study-of-4-money-class-designs-featuring-martin-fowler-kent-beck-and-ward-cunningham-implementations/)?

While the Money as an expression tree holds structural information which is ultimatly rarely necessary, the equality on a Money expression ultimatly needs a in almost all cases an exchange rate if more than one currency is involved. Therefore I do not accept [this argument](https://deque.blog/2017/08/17/a-study-of-4-money-class-designs-featuring-martin-fowler-kent-beck-and-ward-cunningham-implementations/) as an improvement. Especially if we support more than just sums (multiply, distribute) the money bag is very limited.

## Requirements

These is the evolving list of TDD requirements which led to the implementation.

* [x] Use `decimal` `Amount`.
* [x] Support multiple currencies.
* [x] Add two `Money`s in the same `Currency` (5USD + 9USD).
* [x] Cleanup `Currency` (XML handling should be extracted).
* [x] Add two `Money`s with a different `Currency` (5USD + 10CHF).
* [x] Two moneys without Exchange rates should not Evaluate to a result.
* [x] Two moneys are equal if they have the same `Currency` and the same `Amount`.
* [x] Ability to round to 0.05 / distribute 1CHF as [0.35, 0.35, 0.30]
* [x] Evaluation passes through precision to result.
* [x] Override precision on evaluation.
* [x] Support different Exchange rates (evaluation).
* [x] Every construction of `Money` currently rounds to two digits, while this is interesting for 5.7f, it has bad effects in evaluation. We should remove the rounding again.
* [x] The default `MidpointRounding` mechanism is bankers rounding (`MidpointRounding.ToEven`).
* [x] Multiply a `Money` with a real number (`int`, and `decimal`).
* [x] There is a neutral `Money` element (`Zero`).
* [x] Distribute `Money` equally into n slices (1CHF into 3 slices: [0.33, 0.33, 0.34]).
* [x] Distribute `Money` proportionally (1 CHF in 1:5 -> [0.17, 0.83]).
* [ ] Support different distribution strategies?
* [x] Support [ISO 4217](https://en.wikipedia.org/wiki/ISO_4217) Currencies.
* [x] Support calculations smaller than the minor unit? (on `Money` and `EvaluationVisitor`)
* [x] `ToString` supports correct cultural formatting and units.
* [x] Parse `Money` from string considering cultural formatting and units.
* [x] Support operators on the IMoneyExpression interface.
* [x] Convert currencies as late as possible (keep Moneybags per currency in the `EvaluationVisitor`).
* [x] Static constructor for most used Currencies, this could inject rules like precision: Money.CHF(2.00m)
* [x] To avoid rounding problems on construction, Money can only be constructed from decimal and int.
* [x] Add possibility to delegate the acquisition of exchange rates. (`IBank` interface)
* [ ] There are a few throw `Exception` calls in the code which should be refined to specific exceptions.
* [ ] There needs to be a `NoRounding` strategy, maybe provide an Interface `IRoundingStrategy` with a few given implementations.
* [ ] Evaluation arithmetic `Money` operations can use different rounding mechanism (`MidpointRounding`).
* [ ] Distribution with `NoRounding` strategy should distribute exactly.
* [x] Distribution wich cannot exactly distribute money throws an `ImpossibleDistributionException`
* [x] Rounding only happens at the end of an evaluation.
### Decisions

* We construct `Money` objects only from `decimal` and `int`. The decision how to handle external rounding problems should be done before construction of a `Money` object.
* We keep Add, Multiply,etc because no all supported frameworks allow default implementations on the interface.

### Open Decisions

* Implicit type conversion
  * Should A `Money` be constructible implicitly from a `decimal`?
  * Should adding a number to a money be possible (fiveDollars + 2.00m)?
* Rounding
  * Should we round at the end of an evaluation?
  * Should we allow to inject arbitray Rounding Functions a => a?
  * Should a user be able to construct a `Money` which does not conform to the Precision?
* Should we have a substract and divide? We have negative numbers and fractions anyway.

