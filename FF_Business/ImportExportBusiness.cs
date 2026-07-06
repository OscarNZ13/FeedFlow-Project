using FF.Architecture.Dtos;
using FF_ModelsDB.Models;
using System.Text.Json;

namespace FF_Business
{
    public interface IImportExportBusiness
    {
        Task<bool> ImportAsync(SourcePackageDto package);
        Task<SourcePackageDto?> ExportAsync(int sourceId);
        Task<SourcePackageDto?> ExportItemAsync(int sourceItemId);
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

        public async Task<bool> ImportAsync(SourcePackageDto package)
        {
            if (package == null ||
                package.Source == null ||
                package.Items == null)
            {
                return false;
            }

            var source = await _sourceBusiness
                .GetByUrlAsync(package.Source.Url);

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
                string json =
                    JsonSerializer.Serialize(item);


                await _sourceItemBusiness
                    .SaveJsonAsync(
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
                .Select(x =>
                    JsonSerializer.Deserialize<SourceItemDto>(x.Json)
                )
                .Where(x => x != null)
                .ToList();

            SourcePackageDto package = new SourcePackageDto()
            {
                Source = new SourceDto()
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
        public async Task<SourcePackageDto?> ExportItemAsync(int sourceItemId)
        {
            var item = await _sourceItemBusiness
                .GetWithSourceAsync(sourceItemId);

            if (item == null ||
                item.Source == null)
            {
                return null;
            }

            var sourceItem =
                JsonSerializer.Deserialize<SourceItemDto>(
                    item.Json
                );

            if (sourceItem == null)
            {
                return null;
            }

            SourcePackageDto package =
                new SourcePackageDto()
                {
                    Source = new SourceDto()
                    {
                        Url = item.Source.Url,

                        Name = item.Source.Name,

                        Description = item.Source.Description,

                        ComponentType = item.Source.ComponentType,

                        RequiresSecret = item.Source.RequiresSecret
                    },

                    Items = new List<SourceItemDto>()
                    {
                sourceItem
                    }
                };

            return package;
        }

    }
}