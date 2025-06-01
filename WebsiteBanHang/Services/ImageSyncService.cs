// Services/ImageSyncService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebsiteBanHang.Repositories;

namespace WebsiteBanHang.Services
{
    public class ImageSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ImageSyncService> _logger;

        public ImageSyncService(IServiceProvider serviceProvider, ILogger<ImageSyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SyncProductImages();

                    // Chạy mỗi 30 phút
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing product images");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task SyncProductImages()
        {
            using var scope = _serviceProvider.CreateScope();
            var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
            var imageRepo = scope.ServiceProvider.GetRequiredService<IProductImageRepository>();

            _logger.LogInformation("Starting product image sync...");

            var products = await productRepo.GetAllAsync();
            int syncedCount = 0;

            foreach (var product in products)
            {
                try
                {
                    var mainImage = await imageRepo.GetMainImageByProductIdAsync(product.Id);

                    if (mainImage != null && product.ImageUrl != mainImage.Url)
                    {
                        product.ImageUrl = mainImage.Url;
                        await productRepo.UpdateAsync(product);
                        syncedCount++;
                        _logger.LogDebug($"Synced image for product {product.Id}: {product.Name}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to sync image for product {product.Id}");
                }
            }

            if (syncedCount > 0)
            {
                _logger.LogInformation($"Successfully synced {syncedCount} product images");
            }
        }
    }
}

// ✅ Đăng ký service trong Program.cs
// Thêm dòng này vào Program.cs:
// builder.Services.AddHostedService<ImageSyncService>();