#region Instructions
/*
 * You are tasked with writing an algorithm that determines the value of a used car, 
 * given several factors.
 *
 *    AGE:    Given the number of months of how old the car is, reduce its value one-half 
 *            (0.5) percent.
 *            After 10 years, it's value cannot be reduced further by age. This is not 
 *            cumulative.
 *            
 *    MILES:    For every 1,000 miles on the car, reduce its value by one-fifth of a
 *              percent (0.2). Do not consider remaining miles. After 150,000 miles, it's 
 *              value cannot be reduced further by miles.
 *            
 *    PREVIOUS OWNER:    If the car has had more than 2 previous owners, reduce its value 
 *                       by twenty-five (25) percent. If the car has had no previous  
 *                       owners, add ten (10) percent of the FINAL car value at the end.
 *                    
 *    COLLISION:        For every reported collision the car has been in, remove two (2) 
 *                      percent of it's value up to five (5) collisions.
 *
 *    RELIABILITY:      If the Make is Toyota, add 5%.  If the Make is Ford, subtract $500.
 *
 *
 *    PROFITABILITY:    The final calculated price cannot exceed 90% of the purchase price. 
 *    
 * 
 *    Each factor should be off of the result of the previous value in the order of
 *        1. AGE
 *        2. MILES
 *        3. PREVIOUS OWNER
 *        4. COLLISION
 *        5. RELIABILITY
 *        
 *    E.g., Start with the current value of the car, then adjust for age, take that  
 *    result then adjust for miles, then collision, previous owner, and finally reliability. 
 *    Note that if previous owner, had a positive effect, then it should be applied 
 *    AFTER step 5. If a negative effect, then BEFORE step 5.
 */
#endregion

using System;
using NUnit.Framework;

namespace CarPricer
{
    public class Car
    {
        public decimal PurchaseValue { get; set; }
        public int AgeInMonths { get; set; }
        public int NumberOfMiles { get; set; }
        public int NumberOfPreviousOwners { get; set; }
        public int NumberOfCollisions { get; set; }
        public string Make { get; set; }
    }

    public class PriceDeterminator
    { 
        public decimal DetermineCarPrice(Car car)
        {
             const float NOPREVOWNERPERC = 10f;
            decimal purchasePrice = CalcValGivenAge(car.AgeInMonths, car.PurchaseValue);
            purchasePrice = CalcValGivenMiles(car.NumberOfMiles, purchasePrice);
            purchasePrice = CalcValGivenOwners((byte)car.NumberOfPreviousOwners, purchasePrice);
            purchasePrice = CalcValGivenColls((byte)car.NumberOfCollisions, purchasePrice);
            purchasePrice = CalcValGivenReliable(car.Make, purchasePrice);
            if (car.NumberOfPreviousOwners == 0)
            {
                purchasePrice = purchasePrice + purchasePrice * (decimal)(NOPREVOWNERPERC / 100f);
            }
            return CalcValGivenCutoff(purchasePrice, car.PurchaseValue);       
        }
        internal decimal CalcValGivenAge(int age, decimal purchasePrice)
        {
            const float AGEPERCENT = 0.5f;
            const byte TENYEARMONS = 120;
            if (age >= TENYEARMONS)
                age = TENYEARMONS-1;
            return purchasePrice - purchasePrice * (decimal)(age * AGEPERCENT/100.0f); 
        }

        internal decimal CalcValGivenMiles(int mileage, decimal purchasePrice)
        {
            const int MILEAGELIMIT = 150000;
            const int MILEINCS = 1000;
            const float MILEPERCENT = 0.2f;
            if (mileage > MILEAGELIMIT)
                mileage = MILEAGELIMIT;
            return purchasePrice - purchasePrice * (decimal)((mileage / MILEINCS) * MILEPERCENT / 100f);
        }

        internal decimal CalcValGivenOwners(byte prevOwners, decimal purchasePrice)
        {
            const byte PREVOWNERLIMIT = 2;
            const float PREVOWNERPERC = 25f;
            if (prevOwners > PREVOWNERLIMIT)
            {
                purchasePrice = purchasePrice - purchasePrice * (decimal)(PREVOWNERPERC / 100f);
            }
            return purchasePrice;
        }

        internal decimal CalcValGivenColls(byte collisions, decimal purchasePrice)
        {
            const byte MAXCOLLISIONS = 5;
            const float COLLISIONPERC = 2f;

            if (collisions <= MAXCOLLISIONS)
            {
                purchasePrice = purchasePrice - purchasePrice * (decimal)((COLLISIONPERC * collisions)/100f);
            }
            return purchasePrice;
        }
        internal decimal CalcValGivenReliable(string make, decimal purchasePrice)
        {
            const float TOYOTAMAKEPERC = 5f;
            const int FORDMAKESUB = 500;
            if (String.Compare(make?.ToLower(),"toyota") == 0)
            {
                purchasePrice=purchasePrice+purchasePrice * (decimal)((TOYOTAMAKEPERC / 100f));
            }
            else if (String.Compare(make?.ToLower(), "ford") == 0)
            {
                purchasePrice = purchasePrice - FORDMAKESUB;
            }
            return purchasePrice;
        }
        internal decimal CalcValGivenCutoff(decimal purchasePrice, decimal realPurchasePrice)
        {
            const float MAXPROFITPERC = 90;
            decimal ninetyPerCutoff = realPurchasePrice * (decimal)(MAXPROFITPERC / 100f);
            if (purchasePrice < ninetyPerCutoff)
            {
                return purchasePrice;
            }
            else
            {
                return ninetyPerCutoff;
            }
        }

    }
    

    [TestFixture]
    public class UnitTests
    {

        [Test]
        public void CalculateCarValue()
        {
            AssertCarValue(24813.40m, 35000m, 3 * 12, 50000, 1, 1, "Ford");
            AssertCarValue(20672.61m, 35000m, 3 * 12, 150000, 1, 1, "Toyota");
            AssertCarValue(19688.20m, 35000m, 3 * 12, 250000, 1, 1, "Tesla");
            AssertCarValue(21094.5m, 35000m, 3 * 12, 250000, 1, 0, "toyota");
            AssertCarValue(21657.02m, 35000m, 3 * 12, 250000, 0, 1, "Acura");
            AssertCarValue(72000m, 80000m, 8, 10000, 0, 1, null);
        }

        [Test]
        public void TestCalcAgeValue()
        {
            PriceDeterminator priceCalculator = new PriceDeterminator();
            byte ageInMonths = 12;
            decimal purchasePrice = 25000m;
            Assert.AreEqual(priceCalculator.CalcValGivenAge(ageInMonths, purchasePrice), 23500);
            ageInMonths = 120;
            Assert.AreEqual(priceCalculator.CalcValGivenAge(ageInMonths, purchasePrice), 10125);
        }

        [Test]
        public void TestCalcMileageValue()
        {
            PriceDeterminator priceCalculator = new PriceDeterminator();
            int mileage = 12354;
            decimal purchasePrice = 34000m;
            Assert.AreEqual(priceCalculator.CalcValGivenMiles(mileage, purchasePrice), 33184);
        }

        [Test]
        public void TestCalcPrevOwner()
        {
            PriceDeterminator priceCalculator = new PriceDeterminator();
            byte prevOwners = 3;
            decimal purchasePrice = 25034m;
            Assert.AreEqual(priceCalculator.CalcValGivenOwners(prevOwners, purchasePrice), 18775.50);
        }


        [Test]
        public void TestCalcCollisions()
        {
            PriceDeterminator priceCalculator = new PriceDeterminator();
            byte collisions = 3;
            decimal purchasePrice = 29786m;
            Assert.AreEqual(priceCalculator.CalcValGivenColls(collisions, purchasePrice), 27998.84);
        }

        [Test]
        public void TestCalcReliability()
        {
            PriceDeterminator priceCalculator = new PriceDeterminator();
            string make = "Toyota";
            decimal purchasePrice = 39786m;
            Assert.AreEqual(priceCalculator.CalcValGivenReliable(make, purchasePrice), 41775.30);
        }

        [Test]
        public void TestCalcProfit()
        {
            PriceDeterminator priceCalculator = new PriceDeterminator();
            decimal purchasePrice = 41186m;
            decimal realPurchasePrice = 45344m;
            Assert.AreEqual(priceCalculator.CalcValGivenCutoff(purchasePrice, realPurchasePrice),40809.6);
        }
        private static void AssertCarValue(decimal expectValue, decimal purchaseValue,
        int ageInMonths, int numberOfMiles, int numberOfPreviousOwners, int
        numberOfCollisions, string make)
        {
            Car car = new Car
            {
                AgeInMonths = ageInMonths,
                NumberOfCollisions = numberOfCollisions,
                NumberOfMiles = numberOfMiles,
                NumberOfPreviousOwners = numberOfPreviousOwners,
                PurchaseValue = purchaseValue,
                Make = make
            };
            PriceDeterminator priceDeterminator = new PriceDeterminator();
            var carPrice = priceDeterminator.DetermineCarPrice(car);
            Assert.AreEqual(expectValue, carPrice);
        }
        static void Main(string[] args)
        {
            UnitTests test = new UnitTests();
            test.TestCalcAgeValue();
            test.TestCalcMileageValue();
            test.TestCalcPrevOwner();
            test.TestCalcCollisions();
            test.TestCalcReliability();
            test.TestCalcProfit();
            test.CalculateCarValue();
        }
    }
}