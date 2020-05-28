﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RapidCMS.Core.Abstractions.Data;
using RapidCMS.Core.Abstractions.Repositories;
using RapidCMS.Core.Abstractions.Services;
using RapidCMS.Core.Conventions;
using RapidCMS.Core.Enums;
using RapidCMS.Core.Exceptions;
using RapidCMS.Core.Models.ApiBridge.Request;
using RapidCMS.Core.Models.ApiBridge.Response;
using RapidCMS.Core.Models.Request.Api;
using RapidCMS.Core.Models.Response;
using RapidCMS.Core.Models.State;

namespace RapidCMS.Core.Controllers
{
    public class ApiRepositoryController<TEntity, TRepository> : ControllerBase
        where TEntity : class, IEntity
        where TRepository : IRepository
    {
        private readonly IPresentationService _presentationService;
        private readonly IInteractionService _interactionService;

        public ApiRepositoryController(
            IPresentationService presentationService,
            IInteractionService interactionService)
        {
            _presentationService = presentationService;
            _interactionService = interactionService;
        }

        public string CollectionAlias
        {
            get => (string)ControllerContext.ActionDescriptor.Properties[CollectionControllerRouteConvention.CollectionAliasKey];
        }

        // TODO: validation? -> some synchronisation of the changed properties is required
        // without allowing the API to tell what needs to be validated
        // TODO: parentId + IQuery + variant + pagination
        // TODO: remove all logic and put it in a general class for reuse to support things like Azure Functions
        // TODO: mapped variation?
        // TODO: order

        [HttpPost("entity/{id}")]
        public async Task<ActionResult<IEntity>> GetByIdAsync(string id, [FromBody] ParentQueryModel query)
        {
            try
            {
                var entity = await _presentationService.GetEntityAsync<GetEntityRequestModel, IEntity>(new GetEntityRequestModel
                {
                    Subject = new EntityDescriptor
                    {
                        CollectionAlias = CollectionAlias,
                        Id = id,
                        ParentPath = query.ParentPath,
                    },
                    UsageType = UsageType.Node | UsageType.Edit
                });

                return Ok(entity);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("all")]
        public async Task<ActionResult<EntitiesModel<IEntity>>> GetAllAsync([FromBody] ParentQueryModel query)
        {
            try
            {
                var response = await _presentationService.GetEntitiesAsync<GetEntitiesRequestModel, EntitiesResponseModel>(new GetEntitiesOfParentRequestModel
                {
                    CollectionAlias = CollectionAlias,
                    ParentPath = query.ParentPath,
                    UsageType = UsageType.List | UsageType.Edit,
                    Query = query.Query
                });

                return Ok(new EntitiesModel<IEntity>
                {
                    Entities = response.Entities,
                    MoreDataAvailable = response.MoreDataAvailable
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("all/related")]
        public async Task<ActionResult<EntitiesModel<IEntity>>> GetAllRelatedAsync([FromBody] RelatedQueryModel query)
        {
            try
            {
                var response = await _presentationService.GetEntitiesAsync<GetEntitiesRequestModel, EntitiesResponseModel>(new GetEntitiesOfRelationRequestModel
                {
                    CollectionAlias = CollectionAlias,
                    UsageType = UsageType.List | UsageType.Edit,
                    Query = query.Query,
                    Related = new EntityDescriptor
                    {
                        CollectionAlias = query.Related.CollectionAlias,
                        Id = query.Related.Id
                    }
                });

                return Ok(new EntitiesModel<IEntity>
                {
                    Entities = response.Entities,
                    MoreDataAvailable = response.MoreDataAvailable
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("all/nonrelated")]
        public async Task<ActionResult<EntitiesModel<IEntity>>> GetAllNonRelatedAsync([FromBody] RelatedQueryModel query)
        {
            try
            {
                var response = await _presentationService.GetEntitiesAsync<GetEntitiesRequestModel, EntitiesResponseModel>(new GetEntitiesOfRelationRequestModel
                {
                    CollectionAlias = CollectionAlias,
                    UsageType = UsageType.List | UsageType.Add,
                    Query = query.Query,
                    Related = new EntityDescriptor
                    {
                        CollectionAlias = query.Related.CollectionAlias,
                        Id = query.Related.Id
                    }
                });

                return Ok(new EntitiesModel<IEntity>
                {
                    Entities = response.Entities,
                    MoreDataAvailable = response.MoreDataAvailable
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("new")]
        public async Task<ActionResult<IEntity>> NewAsync([FromBody] ParentQueryModel query)
        {
            try
            {
                var entity = await _presentationService.GetEntityAsync<GetEntityRequestModel, IEntity>(new GetEntityRequestModel
                {
                    Subject = new EntityDescriptor
                    {
                        CollectionAlias = CollectionAlias,
                        ParentPath = query.ParentPath,
                        VariantAlias = query.VariantTypeName
                    },
                    UsageType = UsageType.Node | UsageType.New
                });

                return Ok(entity);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("entity")]
        public async Task<ActionResult<IEntity>> InsertAsync([FromBody] EditContextModel<TEntity> editContextModel)
        {
            try
            {
                var response = await _interactionService.InteractAsync<PersistEntityRequestModel, ApiCommandResponseModel>(new PersistEntityRequestModel
                {
                    Descriptor = new EntityDescriptor
                    {
                        CollectionAlias = CollectionAlias,
                        ParentPath = editContextModel.ParentPath
                    },
                    Entity = editContextModel.Entity,
                    EntityState = EntityState.IsNew
                }, ViewState.Api);

                return Ok(((NewEntityApiCommandResponseModel)response).NewEntity);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("entity/{id}")]
        public async Task<ActionResult> UpdateAsync(string id, [FromBody] EditContextModel<TEntity> editContextModel)
        {
            try
            {
                await _interactionService.InteractAsync<PersistEntityRequestModel, ApiCommandResponseModel>(new PersistEntityRequestModel
                {
                    Descriptor = new EntityDescriptor
                    {
                        CollectionAlias = CollectionAlias,
                        Id = id,
                        ParentPath = editContextModel.ParentPath
                    },
                    Entity = editContextModel.Entity,
                    EntityState = EntityState.IsExisting
                }, ViewState.Api);

                return Ok();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("entity/{id}")]
        public async Task<ActionResult> DeleteAsync(string id, [FromBody] DeleteModel delete)
        {
            try
            {
                await _interactionService.InteractAsync<DeleteEntityRequestModel, ApiCommandResponseModel>(new DeleteEntityRequestModel
                {
                    Descriptor = new EntityDescriptor
                    {
                        CollectionAlias = CollectionAlias,
                        Id = id,
                        ParentPath = delete.ParentPath
                    }
                }, ViewState.Api);

                return Ok();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("relate")]
        public async Task<ActionResult> AddRelationAsync([FromBody] RelateModel relate)
        {
            try
            {
                await _interactionService.InteractAsync<PersistRelatedEntityRequestModel, ApiCommandResponseModel>(new PersistRelatedEntityRequestModel
                {
                    Related = new EntityDescriptor
                    {
                        CollectionAlias = relate.Related.CollectionAlias,
                        Id = relate.Related.Id
                    },
                    Subject = new EntityDescriptor
                    {
                        CollectionAlias = CollectionAlias,
                        Id = relate.Id
                    },
                    Action = PersistRelatedEntityRequestModel.Actions.Add
                }, ViewState.Api);

                return Ok();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("relate")]
        public async Task<ActionResult> RemoveRelationAsync([FromBody] RelateModel relate)
        {
            try
            {
                await _interactionService.InteractAsync<PersistRelatedEntityRequestModel, ApiCommandResponseModel>(new PersistRelatedEntityRequestModel
                {
                    Related = new EntityDescriptor
                    {
                        CollectionAlias = relate.Related.CollectionAlias,
                        Id = relate.Related.Id
                    },
                    Subject = new EntityDescriptor
                    {
                        CollectionAlias = CollectionAlias,
                        Id = relate.Id
                    },
                    Action = PersistRelatedEntityRequestModel.Actions.Remove
                }, ViewState.Api);

                return Ok();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("reorder")]
        public async Task<ActionResult> ReorderAsync([FromBody] ReorderModel reorder)
        {
            try
            {
                await _interactionService.InteractAsync<PersistReorderRequestModel, ApiCommandResponseModel>(new PersistReorderRequestModel
                {
                    BeforeId = reorder.BeforeId,
                    Subject = new EntityDescriptor
                    {
                        Id = reorder.Subject.Id,
                        CollectionAlias = CollectionAlias,
                        ParentPath = reorder.Subject.ParentPath
                    }
                }, ViewState.Api);

                return Ok();
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
