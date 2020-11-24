# funcky-money

Funcky.Money is an implementation 

A Money class, based on the TDD implementation of Kent Beck.

Why not a [money bag](https://deque.blog/2017/08/17/a-study-of-4-money-class-designs-featuring-martin-fowler-kent-beck-and-ward-cunningham-implementations/)?

While the Money as an expression tree holds structural information which is ultimatly rarely necessary, the equality on a Money expression ultimatly needs a in almost all cases an exchange rate if more than one currency is involved. Therefore I do not accept [this argument](https://deque.blog/2017/08/17/a-study-of-4-money-class-designs-featuring-martin-fowler-kent-beck-and-ward-cunningham-implementations/) as an improvement. Especially if we support more than just sums (multiply, distribute) the money bag is very limited.

Requirements

* [x] Use `decimal` `Amount`
* [x] Support multiple currencies
* [x] Add two `Money`s in the same `Currency` (5USD + 9USD)
* [x] It should be possible to construct a `Money` from  a `double`.
* [ ] Constructing A Money from float should give the expceted value (5.7f => 5.70m) 
* [ ] Cleanup `Currency`. (XML handling should be extracted)
* [x] Add two `Money`s with a different `Currency` (5USD + 10CHF)
* [x] Two moneys without Exchange rates should not Evaluate to a result
* [x] Two moneys are equal if they have the same `Currency` and the same `Amount`
* [ ] Support different Exchange rates (evaluation)
* [ ] Evaluating arithmetic `Money` operations can use different rounding mechanism (`MidpointRounding`)
* [ ] The default `MidpointRounding` mechanism is bankers rounding (`MidpointRounding.ToEven`)
* [X] Multiply a `Money` with a real number (`int`, `double`, `float` and `decimal`)
* [ ] There is a neutral `Money` element (`Zero`)
* [ ] Respect the  [Monadic Laws](https://blog.ploeh.dk/2017/10/16/money-monoid/)?
* [ ] Distribute `Money` equally into n slices (1CHF into 3 slices: [0.33, 0.33, 0.34])
* [ ] Distribute `Money` proportionally (1 CHF in 1:5 -> [0.17, 0.83]
* [ ] Support different distribution strategies?
* [x] Support [ISO 4217](https://en.wikipedia.org/wiki/ISO_4217) Currencies
* [ ] Support Minor unit
* [ ] Support calculations smaller than the minor unit? (on `Money` and Evaluation)
* [ ] ToString supports correct cultural formatting and units.
* [ ] Parse `Money` from string considering cultural formatting and units.
* [ ] Should A `Money` be constructible implicitly?
* [ ] Should we have a substract and divide? (We have negative numbers and fractions anyway)
