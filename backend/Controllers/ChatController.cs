using System.Text.Json;
using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatGptService _chatGptService;
        private readonly BookingService _bookingService;
        private readonly ChatMessageService _chatMessageService;

        public ChatController(
            ChatGptService chatGptService,
            BookingService bookingService,
            ChatMessageService chatMessageService)
        {
            _chatGptService = chatGptService;
            _bookingService = bookingService;
            _chatMessageService = chatMessageService;
        }
        public record BookingRequestObj(string Resource, DateTime StartTime, DateTime EndTime, Guid UserId, string UserName);
        public record BookingInfoObj(string Resource, DateTime StartTime, DateTime EndTime, Guid UserId, string UserName, string Status);

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDTO request)
        {
            if (request == null) return Ok(new { response = "Det blev något fel. Kan du säga det en gång till?" });

            // Store the user's message in database
            await _chatMessageService.StoreMessage(request.UserId, request.UserInput, "User");

            // Analyze the user's intent from user input
            var jsonString = await _chatGptService.AnalyzeUserIntent(request);

            var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;
            
            var intent = root.GetProperty("intent").ToString();
            var userRequest = root.GetProperty("request").ToString();

            // Provide a reply based on the user's intent 
            switch (intent)
            {
                case "initialize_booking":

                    var replyOptions = await _chatGptService.ProvideOptions(request);

                    await _chatMessageService.StoreMessage(request.UserId, replyOptions, "AI");
                    return Ok(new { response = replyOptions });

                case "confirm_booking":

                    BookingRequestObj? bookingRequestObj = JsonSerializer.Deserialize<BookingRequestObj>(userRequest);

                    var result = await _chatGptService.ConfirmBooking(bookingRequestObj, request.UserInput);

                    if (result == null || result == "")
                    {
                        var confirmBookingErrorMessage = "Det blev något fel på min sida. Kan du säga det en gång till?";
                        await _chatMessageService.StoreMessage(bookingRequestObj.UserId, confirmBookingErrorMessage, "AI");
                        return Ok(new { response = confirmBookingErrorMessage });
                    }

                    var resultDoc = JsonDocument.Parse(result);
                    var resultRoot = resultDoc.RootElement;
                    var reply = resultRoot.GetProperty("reply").ToString();
                    var bookingInfoJson = resultRoot.GetProperty("bookingInfo").ToString();

                    var bookingInfoObj = JsonSerializer.Deserialize<BookingInfoObj>(bookingInfoJson);

                    if (bookingInfoObj.Status == "Confirmed")
                    {
                        var errorMessage = await _bookingService.HandleBooking(bookingInfoObj);

                        if (errorMessage == "")
                        {
                            await _chatMessageService.StoreMessage(bookingRequestObj.UserId, reply, "AI");
                            return Ok(new { response = reply });
                        }

                        await _chatMessageService.StoreMessage(bookingRequestObj.UserId, errorMessage, "AI");
                        return Ok(new { response = errorMessage });
                    }

                    await _chatMessageService.StoreMessage(bookingRequestObj.UserId, reply, "AI");
                    return Ok( new { response = reply });

                case "update_booking_info":

                    var replyUpdate = await _chatGptService.ProvideOptions(request);

                    await _chatMessageService.StoreMessage(request.UserId, replyUpdate, "AI");
                    return Ok( new { response = replyUpdate });

                default:

                    var replyAskToClarify = "Jag är inte säker på att jag förstod. Skulle du kunna förklara det lite tydligare?";
                    await _chatMessageService.StoreMessage(request.UserId, replyAskToClarify, "AI");
                    return Ok( new { response = replyAskToClarify });
            }
        }
    }
}
