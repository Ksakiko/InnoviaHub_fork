using System;
using System.Text;
using System.Text.Json;
using backend.DTOs;
using backend.Models.Interfaces;
using static backend.Controllers.ChatController;

namespace backend.Services;



public class ChatGptService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IBookingRepository _bookingRepository;

    public ChatGptService(
        IHttpClientFactory httpClientFactory,
        IChatMessageRepository chatMessageRepository,
        IResourceRepository resourceRepository,
        IBookingRepository bookingRepository)
    {
        _httpClientFactory = httpClientFactory;
        _chatMessageRepository = chatMessageRepository;
        _resourceRepository = resourceRepository;
        _bookingRepository = bookingRepository;
    }

    public record ExtractedBookingRequest(string Resource, DateTime? StartTime, DateTime? EndTime);

    // public async Task<ExtractedBookingRequest?> ExtractUserBookingRequest(ChatRequestDTO request)
    // {
    //     var allMessages = await _chatMessageRepository.GetAllAsync();
    //     var latestMessages = allMessages
    //     .Where(x => x.UserId == request.UserId)
    //     .OrderBy(x => x.CreatedAt)
    //     .TakeLast(5)
    //     .ToList();
       
    //     var today = DateTime.UtcNow;
    //     var tomorrow = today.AddDays(1);

    //     var http = _httpClientFactory.CreateClient("openai");

    //     var body = new
    //     {
    //         model = "gpt-4.1",
    //         input = new object[]
    //         {
    //             new {
    //                 role = "system",
    //                 content = $@"Extract resource name and start time from user input for a resource booking system.

    //                     Extract booking info from user input and return JSON in this example format:
    //                     {{
    //                         ""Resource"": ""Example Resource"",
    //                         ""StartTime"": ""yyyy-MM-ddT00:00:00.000Z"",
    //                         ""EndTime"": ""yyyy-MM-ddT23:59:59.999Z"",
    //                     }}

    //                     - If the user says ""tomorrow"", interpret it as {tomorrow} for StartTime
    //                     - If the user says ""next [day of the week]"", interprest it as upcoming day of the week.
    //                         Examples:
    //                             - If the user says next Thursday and {today} is Thursday, you should refer to next comming Thurday for the startTime.
    //                             - If the user says next Friday and today is Tuesday, interpret it as not Friday that comes in 3 days but 10 days. 
    //                     - The time section in startTime should always be ""00:00:00.000Z""
    //                     - The time section in endTime should always be ""23:59:59.999Z""
    //                     - If the user says ""skrivbord"" without number afterwards, convert it to ""Dropin-skrivbord""
    //                     - If the user says ""vr"" or ""VR"" followed by a number, interpret it as ""VR-glasögon"" otherwise ""VR-headset""
    //                     - If the user says, for example, ""ai"", interpret it as ""AI-server""
    //                     - If the user says, for example, ""mötesrum 1"" and ""vr1"", convert them to ""Mötesrum 1"" and ""VR-glasögon 1"" for Resource in JSON
    //                     - If the user input does not provide Resource for the JSON-format, refer to {latestMessages} and find Resource from messages that mentions ""boka"" or ""book"" 
    //                     - If the user input does not provide StartTime or EndTime, apply the current date
    //                     - Resource in JSON must always be capitalized
    //                     ",
    //             },
    //             new {
    //                 role = "user",
    //                 content = request.UserInput
    //             }
    //         }
    //     };

    //     var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    //     var response = await http.PostAsync("responses", content);

    //     var raw = await response.Content.ReadAsStringAsync();

    //     var doc = JsonDocument.Parse(raw);
    //     var root = doc.RootElement;
 
    //     var jsonString = root.GetProperty("output")[0].GetProperty("content")[0].GetProperty("text").GetString();

    //     if (jsonString == "" || jsonString == null) return null;

    //     ExtractedBookingRequest? responseItem = JsonSerializer.Deserialize<ExtractedBookingRequest>(jsonString);

    //     if (responseItem == null) return null;

    //     return responseItem;
    // }

    public async Task<string> AnalyzeUserIntent(ChatRequestDTO request)
    {
        var allMessages = await _chatMessageRepository.GetAllAsync();

        var latestMessages = allMessages
        .Where(x => x.UserId == request.UserId)
        .OrderBy(x => x.CreatedAt)
        .TakeLast(7)
        .ToList();

        var http = _httpClientFactory.CreateClient("openai");

        var body = new
        {
            model = "gpt-4.1",
            input = new object[]
            {
                new {
                    role = "system",
                    content = $@"You are a booking assistant who analyzes user's intent from user inputs.
                    - Refer to the following messages to analyze and only base the response on this data.
                    Messages: {JsonSerializer.Serialize(latestMessages)}

                    - The user can have one of the following intents:
                    - ""initialize_booking"",
                    - ""confirm_booking"",
                    - ""update_booking_info"",

                    
                    - If the user says only ""ja"" or ""tack"", check the last message in Messages and find out the user's intent
                    - If the user's intent is confirm_booking, extract the resource name and StartTime from the message.
                    - The date of EndTime is same as StartTime
                    - If the user's intent is update_booking_info, extract the updated date or/and the updated resource name from the user's input and apply on StartTime and EndTime
                    - If Resource, StartTime or EndTime is missing from the user input, refer to the latest messages in Messages  
                    - Resource name in JSON should always be capitalized

                    The response format should be the following if the users intent is one of the following: 
                    - confirm_booking
                    - update_booking_info

                    {{
                        ""intent"": ""confirm_booking"",
                        ""request"": {{
                                        ""Resource"": ""Example Resource"",
                                        ""StartTime"": ""yyyy-MM-ddT00:00:00.000Z"",
                                        ""EndTime"": ""yyyy-MM-ddT23:59:59.999Z"",
                                        ""UserId"": {request.UserId},
                                        ""UserName"": {request.UserName}
                                    }}
                    }}
                    

                    - Example JSON in the case that the user's intent is initialize_booking
                    {{
                        ""intent"": ""initialize_booking"",
                        ""request"": {{
                                        ""UserInput""{request.UserInput}
                                        ""UserId""{request.UserId}
                                        ""UserName""{request.UserName}
                                     }}
                    }}

                    Apply the same format but don't copy the data values from here, it needs to be based on user input only.
                    Replace ""Example Resource"" with the resource the user wants to book. Replace StartTIme and EndTime with the  corresponding
                    values the user has provided.

                    ",
                },
                new {
                    role = "user",
                    content = request.UserInput
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await http.PostAsync("responses", content);

        var raw = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;

        var jsonString = root.GetProperty("output")[0].GetProperty("content")[0].GetProperty("text").ToString();

        return jsonString;
    }

    public async Task<string> ProvideOptions(ChatRequestDTO request)
    {
        var allMessages = await _chatMessageRepository.GetAllAsync();

        var latestMessages = allMessages
        .Where(x => x.UserId == request.UserId)
        .OrderBy(x => x.CreatedAt)
        .TakeLast(10)
        .ToList();
       
        var today = DateTime.UtcNow;
        var tomorrow = today.AddDays(1);

        var bookings = _bookingRepository.GetBookingsByUserId(request.UserId);

        var resources = await _resourceRepository.GetAll();

        var http = _httpClientFactory.CreateClient("openai");

        var body = new
        {
            model = "gpt-4.1",
            input = new object[]
            {
                new {
                    role = "system",
                    content = $@"You are a booking assistant to find a resource that the user can book based on the user input.
                        Resources: {JsonSerializer.Serialize(resources)}
                        Messages: {JsonSerializer.Serialize(latestMessages)}
                        ConfirmedBookings: {JsonSerializer.Serialize(bookings)}

                        - If the user says ""tomorrow"", interpret it as {tomorrow} for StartTime
                        - If the user says ""next [day of the week]"", interprest it as upcoming day of the week.
                            Examples:
                                - If the user says next Thursday and {today} is Thursday, you should refer to next comming Thurday for the startTime.
                                - If the user says next Friday and today is Tuesday, it means the Friday that comes in 10 days. 
                                - If the user says next Wednesday and today is Thursday, it means the Wednesday that comes in 6 days. 
                        - You do not need to mention to the user how you calculate the day of the week
                        - Refer to ConfirmedBookings and check if the user has already booked the same resource for the same date and time
                        - If the user has already booked the same resource for the same date, ask the user if he or she wants to change the booking or start a new booking process
                        - If the user has not booked the same resource for the same date, you do not need to mention it 
                        - Find the exact resource that the user has requested and ask the user if he or she wants to book it
                        - If the user did not specify any resource name, refer to Resources and provide available resources that are correct type of resource that the user needs. 
                        - Return a message to provide available resources based on Resources
                        - Include the date when you ask the user if he or she wants to book the resource
                        - If the user did not specify any date first, doublecheck if the date is what the user wants
                        - Use Messages to identify resources that were already suggested and omit them from the resource options
                        - The current date is {today}
                        - If the user input does not provide StartTime or EndTime, apply the current date
                    ",
                },
                new {
                    role = "user",
                    content = request.UserInput
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await http.PostAsync("responses", content);

        var raw = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;

        var jsonString = root.GetProperty("output")[0].GetProperty("content")[0].GetProperty("text").ToString();

        return jsonString;
    }

    public async Task<string> ConfirmBooking(BookingRequestObj request, string userInput)
    {
        var allMessages = await _chatMessageRepository.GetAllAsync();

        var latestMessages = allMessages
        .Where(x => x.UserId == request.UserId)
        .OrderBy(x => x.CreatedAt)
        .TakeLast(10)
        .ToList();


        var http = _httpClientFactory.CreateClient("openai");

        var body = new
        {
            model = "gpt-4.1",
            input = new object[]
            {
                new {
                    role = "system",
                    content = $@"You are a booking assistant to confirm bookings.
                        Messages: {JsonSerializer.Serialize(latestMessages)}
                        UserBooking: {JsonSerializer.Serialize(request)}
                        - Return always a JSON that includes a reply to the user and bookingInfo that follows the format of UserBooking
                        - Add Status to bookingInfo and set the value to Pending before the user agrees for the booking info
                        - Ask the user if the requested booking info is correct before you proceed to booking confirmation
                        - If the user agrees, return a JSON that includes a confirmation message and bookingInfo that the user has confirmed
                        - If the user agrees, change the value of Status to Confirmed
                        - The values in UserBooking should be updated as needed based on the conversation in Messages
                        - Confirmation message should include the booking info
                    ",
                },
                new {
                    role = "user",
                    content = userInput
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        var response = await http.PostAsync("responses", content);

        var raw = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(raw);
        var root = doc.RootElement;

        var jsonString = root.GetProperty("output")[0].GetProperty("content")[0].GetProperty("text").ToString();

        return jsonString;
    }
}
