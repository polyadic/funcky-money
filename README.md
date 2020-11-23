# funcky-money
A Money class, based on the TDD implementation of Kent Beck

TDD
* Use decimal amount
* Add two Moneys in the same currency
* Add two Moneys with a different currency
* Two moneys are equal if they have the same currency and the same amount
* Support different Exchange rates
* Evaluating arithmetic money operations can use different rounding mechanism
* The default rounding mechanism is bankers rounding
* Multiply a Money with a Real number
* There is a neutral Money element
* Respect the Monadic Laws (https://blog.ploeh.dk/2017/10/16/money-monoid/)
* Distribute Money equally into n slices (1CHF into 3 slices: [0.33, 0.33, 0.34])
* Distribute Money proportionally (1 CHF in 1:5 -> [0.17, 0.83]
* Support different distribution strategies?
* Support ISO 4217 currencies
* Support Minor unit
* Support calculations smaller than the minor unit?
* ToString supports correct Cultural formatting and Units.
* Parse Money from String considering cultural formatting and units. 
