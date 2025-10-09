using System.Text.Json;
using backend.DTOs;
using backend.Models.Entities;
using backend.Models.Interfaces;
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
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IBookingRepository _bookingRepository;

        public ChatController(
            ChatGptService chatGptService,
            BookingService bookingService,
            ChatMessageService chatMessageService,
            IChatMessageRepository chatMessageRepository,
            IBookingRepository bookingRepository)
        {
            _chatGptService = chatGptService;
            _bookingService = bookingService;
            _chatMessageService = chatMessageService;
            _chatMessageRepository = chatMessageRepository;
            _bookingRepository = bookingRepository;
        }
        public record BookingRequestObj(string Resource, DateTime StartTime, DateTime EndTime, Guid UserId, string UserName);
        public record BookingInfoObj(string Resource, DateTime StartTime, DateTime EndTime, Guid UserId, string UserName, string Status);

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDTO request)
        {
            // Store user message in database
            var userMessage = new ChatMessage()
            {
                UserId = request.UserId,
                Sender = "User",
                Message = request.UserInput,
                CreatedAt = DateTime.UtcNow
            };

            await _chatMessageRepository.AddAsync(userMessage);

            // Analyze user's intent from user input
            var jsonString = await _chatGptService.AnalyzeUserIntent(request);

            var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;
            var intent = root.GetProperty("intent").ToString();
            var userRequest = root.GetProperty("request").ToString();

            switch (intent)
            {
                case "initialize_booking":
                    // Extract booking info from user input
                    if (request == null) return Ok("Något gick fel. Försök igen.");

                    var bookingRequest = await _chatGptService.ExtractUserBookingRequest(request);

                    if (bookingRequest == null ||
                        bookingRequest.Resource == null ||
                        bookingRequest.StartTime == null ||
                        bookingRequest.EndTime == null)
                    {
                        // Return message to user for invalid input
                        var  replyNotEnoughInfo = "Vänligen ange både önskad resurs och datum.";
                        await _chatMessageService.StoreMessage(request.UserId, replyNotEnoughInfo, "AI");
                        return Ok(replyNotEnoughInfo);
                    }


                    var replyOptions = await _chatGptService.ProvideOptions(request);

                    await _chatMessageService.StoreMessage(request.UserId, replyOptions, "AI");
                    return Ok(replyOptions);

                case "confirm_booking":
                    BookingRequestObj? bookingRequestObj = JsonSerializer.Deserialize<BookingRequestObj>(userRequest);

                    var result = await _chatGptService.ConfirmBooking(bookingRequestObj, request.UserInput);

                    var resultDoc = JsonDocument.Parse(result);
                    var resultRoot = resultDoc.RootElement;
                    var reply = resultRoot.GetProperty("reply").ToString();
                    var bookingInfoJson = resultRoot.GetProperty("bookingInfo").ToString();

                    var bookingInfoObj = JsonSerializer.Deserialize<BookingInfoObj>(bookingInfoJson);

                    if (bookingInfoObj.Status == "Confirmed")
                    {
                        try
                        {
                            var errorMessage = await _bookingService.HandleBooking(bookingInfoObj);

                            if (errorMessage == "") return Ok(reply);

                            return Ok(errorMessage);    
                        } catch
                        {
                            
                        }
                    }

                    await _chatMessageService.StoreMessage(bookingRequestObj.UserId, reply, "AI");

                    return Ok(reply);
                    // return Ok(reply);
                case "see_more_options":
                    return Ok();

                case "update_booking_info":
                    // Ask for confirmation
                    return Ok();

                case "cancel_process":
                    return Ok();

                default:
                    var replyAskToClarify = "Jag förstod inte vad du menade. Skulle du kunna förtydliga det?";
                    await _chatMessageService.StoreMessage(request.UserId, replyAskToClarify, "AI");
                    return Ok(replyAskToClarify);
            }
        }
    }
}
