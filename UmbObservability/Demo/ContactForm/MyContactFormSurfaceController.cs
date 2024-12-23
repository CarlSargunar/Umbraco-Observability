using UmbObservability.Demo.OTel;

namespace UmbObservability.Demo.ContactForm;

public class MyContactFormSurfaceController : SurfaceController
{
    private readonly IEmailSender _emailSender;
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ILogger<MyContactFormSurfaceController> _logger;

    public MyContactFormSurfaceController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IEmailSender emailSender,
        IOptions<GlobalSettings> globalSettings, ILogger<MyContactFormSurfaceController> logger)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _emailSender = emailSender;
        _globalSettings = globalSettings;
        _logger = logger;
    }

    public async Task<IActionResult> Submit(MyContactFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return CurrentUmbracoPage();
        }

        TempData["Message"] = await HandleSuccessfulSubmitAsync(model);

        return RedirectToCurrentUmbracoPage();
    }

    protected virtual async Task<string> HandleSuccessfulSubmitAsync(MyContactFormViewModel model)
    {
        try
        {
            EmailMessage mailMessage = new(_globalSettings.Value?.Smtp?.From ?? "noreply@umbraco.com", model.Email, model.Subject ?? $"Website Contact form: {model.Name}", model.Message, true);
            await _emailSender.SendAsync(mailMessage, "StarterKitContactEmail", true);

            return "Message submitted";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while submitting the form");
            return "An error occurred while submitting the form";
        }
    }
}