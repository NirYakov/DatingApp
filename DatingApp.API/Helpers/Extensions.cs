using System;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            const string appError = "Application-Error";
            // const string accessControl = "Access-Control-";
            response.Headers.Add(appError,message);
            // response.Headers.Add($"{accessControl}Expose-Headers",appError);
            response.Headers.Add("Access-Control-Expose-Headers",appError);
            response.Headers.Add("Access-Control-Allow-Origin","*");
            
        }

        public static int CalculateAge(this DateTime theDateTime)
        {
            var age = DateTime.Today.Year - theDateTime.Year;
            if(theDateTime.AddYears(age) > DateTime.Today){
                age--;
            }

            return age;
        }
    }
}