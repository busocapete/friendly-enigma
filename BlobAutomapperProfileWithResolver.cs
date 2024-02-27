using System;
using AutoMapper;
using BuSoCa.Data.Entities;
using BuSoCa.Data.Services;
using BuSoCa.Model.AzureDtos;
using BuSoCa.Model.Dtos;

namespace BuSoCa.MappingProfiles.Profiles
{
    public class BlobProfile : AutoMapper.Profile
    {
        public BlobProfile()
        {
            //CreateMap<BlobDto, Blob>().ReverseMap();

            CreateMap<Blob, AzureBlob>();

            CreateMap<Blob, BlobDto>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom<LocationResolver>());

            CreateMap<AzureBlob, BlobDto>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom<AzureBlobResolver>());

            CreateMap<Tuple<Guid, Blob>, Tuple<Guid, BlobDto>>()
                .ForMember(dest => dest.Item2, opt => opt.MapFrom(src => src.Item2));

        }
    }

    public class LocationResolver : IValueResolver<Blob, BlobDto, string>
    {
        private readonly IBlobService _blobService;

        public LocationResolver(IBlobService blobService)
        {
            _blobService = blobService;
        }
        public string Resolve(Blob blob, BlobDto blobDto, string location, ResolutionContext context)
        {
            location = Task.Run(() => _blobService.GetBlobUriWithSasToken(blob.ContainerName, blob.Name)).Result;
            return location;
        }
    }
