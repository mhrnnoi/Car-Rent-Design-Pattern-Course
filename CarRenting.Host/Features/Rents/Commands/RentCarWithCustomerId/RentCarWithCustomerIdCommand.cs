﻿using CarRenting.Host.Common;
using CarRenting.Host.Entities;
using CarRenting.Host.Interfaces;
using CarRenting.Host.PricingStrategyCreation;
using CarRenting.Host.RentalService;
using Entities;

namespace CarRenting.Host.Features.Rents.Commands.RentCarWithCustomerId
{
    public class RentCarWithCustomerIdCommand : ICommand<RentalAgreement>
    {
        public int CarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CarType CarType { get; set; }
        public int CustomerId { get; set; }
        private readonly ICarRentalService _carRentalService;
        private readonly IPricingStrategy _pricingStrategy;

        public RentCarWithCustomerIdCommand(int carId, DateTime startDate, DateTime endDate, int customerId, CarType carType)
        {
            CarId = carId;
            StartDate = startDate;
            EndDate = endDate;
            CustomerId = customerId;
            CarType = carType;
            _carRentalService = new CarRentalService();
            _pricingStrategy = PricingStrategyFactory.CreatePricingStrategy(carType);
        }

        public Response<RentalAgreement> Execute()
        {
            Customer? customer = CarRentalSystem.Instance.GetCustomers().Where(c => c.Id == CustomerId).FirstOrDefault();
            if (customer == null)
            {
                return new Response<RentalAgreement>("Customer not found");
            }
            try
            {
                RentalAgreement rentalAgreement = _carRentalService.RentCar(CarId, StartDate, EndDate, customer);
                int days = (EndDate - StartDate).Days;
                rentalAgreement.RentalPrice = _pricingStrategy.CalculatePrice(days);
                CarRentalSystem.Instance.AddRentalAgreement(rentalAgreement);
                return new Response<RentalAgreement>(rentalAgreement);
            }
            catch (Exception e)
            {

                return new Response<RentalAgreement>(e.Message.ToString());
            }
        }
    }
}
