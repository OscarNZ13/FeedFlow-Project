using FF.Architecture.Dtos;
using FF.Architecture.Parsers;
using FF_ModelsDB.Models;
using System.Text.Json;

namespace FF_Business
{
    public interface IImportExportBusiness
    {
        Task<bool> ImportAsync(SourcePackageDto package);
        Task<bool> ImportItemAsync(ExportSourceItemDto item);
        Task<SourcePackageDto?> ExportAsync(int sourceId);
        Task<ExportSourceItemDto?> ExportItemAsync(int sourceItemId);
    }

    public class ImportExportBusiness : IImportExportBusiness
    {
        private readonly ISourceBusiness _sourceBusiness;

        private readonly ISourceItemBusiness _sourceItemBusiness;

        public ImportExportBusiness(ISourceBusiness sourceBusiness, ISourceItemBusiness sourceItemBusiness)
        {
            _sourceBusiness = sourceBusiness;
            _sourceItemBusiness = sourceItemBusiness;
        }

        public async Task<bool> ImportItemAsync(ExportSourceItemDto item)
        {
            if (item == null)
            {
                return false;
            }

            var source = await _sourceBusiness.GetByUrlAsync(item.SourceUrl);

            if (source == null)
            {
                source = new Source()
                {
                    Url = item.SourceUrl,
                    Name = item.SourceName,
                    Description = item.SourceDescription,
                    ComponentType = item.SourceComponentType,
                    RequiresSecret = item.SourceRequiresSecret
                };

                await _sourceBusiness.CreateAsync(source);

                source = await _sourceBusiness.GetByUrlAsync(item.SourceUrl);
            }

            if (source == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(item.Url) &&
                await _sourceItemBusiness.ExistsByUrlAsync(item.Url))
            {
                throw new InvalidOperationException("La noticia ya existe.");
            }

            DateTime? publishedAt = null;

            if (!string.IsNullOrWhiteSpace(item.PublishedAt))
            {
                DateTime fecha;

                if (DateTime.TryParse(item.PublishedAt, out fecha))
                {
                    publishedAt = fecha;
                }
                else if (DateTime.TryParseExact(
                    item.PublishedAt,
                    "d/M/yyyy HH:mm:ss",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out fecha))
                {
                    publishedAt = fecha;
                }
            }

            var newsItem = new NewsItemDto()
            {
                Id = Guid.NewGuid().ToString(),
                Title = item.Title,
                Description = item.Description,
                ImageUrl = item.ImageUrl,
                Url = item.Url,
                PublishedAt = publishedAt,
                SourceName = item.SourceName
            };

            string json = JsonSerializer.Serialize(newsItem);

            return await _sourceItemBusiness.SaveJsonAsync(json, source.Id);
        }

        public async Task<bool> ImportAsync(SourcePackageDto package)
        {
            if (package == null ||
                package.Source == null ||
                package.Items == null)
            {
                return false;
            }

            var source = await _sourceBusiness.GetByUrlAsync(package.Source.Url);

            if (source == null)
            {
                source = new Source()
                {
                    Url = package.Source.Url,
                    Name = package.Source.Name,
                    Description = package.Source.Description,
                    ComponentType = package.Source.ComponentType,
                    RequiresSecret = package.Source.RequiresSecret
                };


                await _sourceBusiness
                    .CreateAsync(source);


                source = await _sourceBusiness
                    .GetByUrlAsync(package.Source.Url);
            }

            foreach (var item in package.Items)
            {
                if (!string.IsNullOrWhiteSpace(item.Url) &&
                    await _sourceItemBusiness.ExistsByUrlAsync(item.Url))
                {
                    continue;
                }

                string json = JsonSerializer.Serialize(item);

                await _sourceItemBusiness.SaveJsonAsync(
                    json,
                    source.Id
                );
            }

            return true;
        }

        public async Task<SourcePackageDto?> ExportAsync(int sourceId)
        {
            var source = await _sourceBusiness
                .GetByIdAsync(sourceId);

            if (source == null)
            {
                return null;
            }

            var items = await _sourceItemBusiness
                .GetBySourceIdAsync(sourceId);

            var sourceItems = items
                .Select(x => JsonSerializer.Deserialize<NewsItemDto>(x.Json))
                .OfType<NewsItemDto>()
                .ToList();

            SourcePackageDto package = new()
            {
                Source = new SourceDto
                {
                    Url = source.Url,
                    Name = source.Name,
                    Description = source.Description,
                    ComponentType = source.ComponentType,
                    RequiresSecret = source.RequiresSecret
                },

                Items = sourceItems
            };
            
            return package;
        }

        public async Task<ExportSourceItemDto?> ExportItemAsync(int sourceItemId)
        {
            var item = await _sourceItemBusiness
                .GetWithSourceAsync(sourceItemId);

            if (item == null || item.Source == null)
            {
                return null;
            }

            var newsItem = JsonSerializer.Deserialize<NewsItemDto>(item.Json);

            if (newsItem == null)
            {
                return null;
            }

            ExportSourceItemDto dto = new ExportSourceItemDto()
            {
                Title = newsItem.Title,
                Description = newsItem.Description,
                ImageUrl = newsItem.ImageUrl,
                Url = newsItem.Url,
                PublishedAt = newsItem.PublishedAt?.ToString("dd/MM/yyyy HH:mm:ss"),

                SourceName = item.Source.Name,
                SourceUrl = item.Source.Url,
                SourceDescription = item.Source.Description,
                SourceComponentType = item.Source.ComponentType,
                SourceRequiresSecret = item.Source.RequiresSecret
            };

            return dto;
        }
    }
}