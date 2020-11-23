# funcky-money
A Money class, based on the TDD implementation of Kent Beck.

TDD
* Use `decimal` amount 
* Add two `Money`s in the same currency (5USD + 9USD)
* Add two `Money`s with a different currency (5USD + 10CHF)
* Two moneys are equal if they have the same currency and the same amount
* Support different Exchange rates (evaluation)
* Evaluating arithmetic money operations can use different rounding mechanism (`MidpointRounding`)
* The default `MidpointRounding` mechanism is bankers rounding (`MidpointRounding.ToEven`)
* Multiply a Money with a Real number (`int`, `double`, `float` and `decimal`)
* There is a neutral Money element (Zero)
* Respect the  [Monadic Laws](https://blog.ploeh.dk/2017/10/16/money-monoid/)?
* Distribute Money equally into n slices (1CHF into 3 slices: [0.33, 0.33, 0.34])
* Distribute Money proportionally (1 CHF in 1:5 -> [0.17, 0.83]
* Support different distribution strategies?
* Support [SO 4217](https://en.wikipedia.org/wiki/ISO_4217) Currencies
* Support Minor unit
* Support calculations smaller than the minor unit? (on Money and Evaluation)
* ToString supports correct Cultural formatting and Units.
* Parse Money from String considering cultural formatting and units. 
