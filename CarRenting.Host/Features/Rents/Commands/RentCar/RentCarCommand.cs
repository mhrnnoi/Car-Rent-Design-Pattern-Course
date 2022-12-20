﻿using CarRenting.Host.Common;
using CarRenting.Host.Entities;
using CarRenting.Host.Interfaces;
using CarRenting.Host.PricingStrategyCreation;
using CarRenting.Host.RentalService;
using Entities;

namespace CarRenting.Host.Features.Rents.Commands.RentCar
{
    public class RentCarCommand : ICommand<RentalAgreement>
    {
        public int CarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public CarType CarType { get; set; }

        private readonly ICarRentalService _carRentalService;
        private readonly IPricingStrategy _pricingStrategy;

        public RentCarCommand(int carId, DateTime startDate, DateTime endDate, string name, string email, string phone, string address, CarType carType)
        {
            CarId = carId;
            StartDate = startDate;
            EndDate = endDate;
            Name = name;
            Email = email;
            Phone = phone;
            Address = address;
            CarType = carType;
            _carRentalService = new CarRentalService();
            _pricingStrategy = PricingStrategyFactory.CreatePricingStrategy(carType);
        }

        public Response<RentalAgreement> Execute()
        {
            Customer customer = new Customer
            {
                Name = Name,
                Email = Email,
                Phone = Phone,
                Address = Address
            };
            try
            {
                RentalAgreement rentalAgreement = _carRentalService.RentCar(CarId, StartDate, EndDate, customer);
                int days = (EndDate - StartDate).Days;

                double rentalPrice = _pricingStrategy.CalculatePrice(days);

                rentalAgreement.RentalPrice = rentalPrice;
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
