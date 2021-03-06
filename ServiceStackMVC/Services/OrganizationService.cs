﻿using ServiceStackMVC.Mappers;
using ServiceStackMVC.Models;
using ServiceStackMVC.Models.DTOs;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceStackMVC.Services
{
    public class OrganizationService : ServiceStack.ServiceInterface.Service
    {
        /// <summary>
        /// GET /organizations/{Id}
        /// </summary>
        public object Get(OrganizationDto request)
        {
            var userSession = base.SessionAs<CustomUserSession>();
            var query = string.Format("select o.*, sp.Name as StateProvinceName, sp.Abbreviation as StateProvinceAbbreviation, oc.Id as OrganizationCategoryId, oc.Name as OrganizationCategoryName " +
                                     "from Organization o " +
                                     "left join StateProvince sp " +
                                     "on o.StateProvinceId = sp.Id " +
                                     "left join OrganizationCategory oc " +
                                     "on o.OrganizationCategoryId = oc.Id " +
                                     "where o.Id = {0}", request.Id);

            var orgDto = Db.Select<OrganizationDto>(query).FirstOrDefault();

            if (orgDto != null)
            {
                //// Categories
                //var orgCategoryQuery = string.Format(
                //    "select oc.* " +
                //    "from OrganizationCategory oc " +
                //    "inner join OrganizationOrganizationCategory ooc " +
                //    "on oc.Id = ooc.OrganizationCategoryId " +
                //    "where ooc.OrganizationId = {0}", orgDto.Id);

                //var orgCategories = Db.Select<OrganizationCategory>(orgCategoryQuery);

                //if (orgCategories != null)
                //{
                //    orgDto.Categories = orgCategories;
                //}

                // Allies
                var alliesCountQuery = string.Format("select count(*) from OrganizationAlly where OrganizationId = {0}", orgDto.Id);
                orgDto.OrganizationAlliesCount = Db.Scalar<int>(alliesCountQuery);

                // News
                var newsCountQuery = string.Format("select count(*) from OrganizationNews where OrganizationId = {0}", orgDto.Id);
                orgDto.OrganizationNewsCount = Db.Scalar<int>(newsCountQuery);

                // Comments
                var commentsCountQuery = string.Format("select count(*) from OrganizationComment where OrganizationId = {0}", orgDto.Id);
                orgDto.OrganizationNewsCount = Db.Scalar<int>(commentsCountQuery);

                //// Events
                //var eventsCountQuery = string.Format("select count(*) from Event where OrganizationId = {0} and StartDateTime >= '{1}'", orgDto.Id, DateTime.Now.ToString());
                //orgDto.OrganizationEventsCount = Db.Scalar<int>(eventsCountQuery);

                if (request.CallerUserId.HasValue)
                {
                    var userFollowingOrgQuery = string.Format("select count(*) from OrganizationAlly where OrganizationId = {0} and UserId = {1}", orgDto.Id, request.CallerUserId);
                    var userFollowingCount = Db.Scalar<int>(userFollowingOrgQuery);
                    orgDto.CallerIsFollowingOrg = userFollowingCount > 0;
                }
            }

            return new OrganizationResponse { Organization = orgDto };
        }

        /// <summary>
        /// Get Organization Users/Followers
        /// GET /organizations/{OrganizationId}/users
        /// </summary>
        public object Get(OrganizationUsers request)
        {
            var query = string.Format("select u.*, sp.Name as StateProvinceName, sp.Abbreviation as StateProvinceAbbreviation " +
                "from User u " +
                "inner join OrganizationAlly oa " +
                "on u.Id = oa.UserId " +
                "left join StateProvince sp " +
                "on u.StateProvinceId = sp.Id " +
                "where oa.OrganizationId = {0}", request.OrganizationId);

            var users = Db.Select<UserDto>(query);

            return new OrganizationUsersResponse
            {
                Users = users
            };
        }

        /// <summary>
        /// Get Organization Comments
        /// GET /organizations/{OrganizationId}/comments
        /// </summary>
        public object Get(OrganizationCommentsDto request)
        {
            var query = string.Format("select u.Id, u.FirstName, u.LastName, oc.CommentText, oc.CreatedAt " +
                                    "from OrganizationComment oc " +
                                    "inner join Organization o " +
                                    "on oc.OrganizationId = o.Id " +
                                    "left join User u " +
                                    "on o.UserID = u.Id " +
                                    "where o.Id = {0}", request.OrganizationId);

            var comments = Db.Select<OrganizationCommentDto>(query);

            return new OrganizationCommentsResponse
            {
                OrganizationComments = comments
            };
        }

        public object Post(OrganizationDto request)
        {
            var organization = request.ToEntity();
            Db.Insert(organization);
            var newOrgId = (int)Db.GetLastInsertId();

            //Db.Delete<OrganizationOrganizationCategory>(ooc => ooc.OrganizationId == newOrgId);

            //if (organizationDto.Categories != null && organizationDto.Categories.Any())
            //{
            //    Db.InsertAll(organizationDto.Categories.Select(c => new OrganizationOrganizationCategory
            //                                                            {
            //                                                                OrganizationCategoryId = c.Id,
            //                                                                OrganizationId = newOrgId
            //                                                            }));
            //}

            return new OrganizationResponse { Organization = new OrganizationDto() };
        }

        /// <summary>
        /// Post Organization Comment
        /// POST organizations/{OrganizationId}/comments
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object Post(OrganizationCommentDto request)
        {
            if (request.User != null &&
                request.User.Id > 0 &&
                request.OrganizationId > 0 &&
                !String.IsNullOrWhiteSpace(request.CommentText) &&
                request.CommentText.Length > 1)
            {
                var comment = new OrganizationComment
                {
                    UserId = request.User.Id,
                    OrganizationId = request.OrganizationId,
                    CommentText = request.CommentText,
                    CreatedAt = DateTime.Now
                };

                Db.Insert(comment);
            }

            return new OrganizationCommentResponse
            {
                OrganizationComment = new OrganizationCommentDto()
            };
        }

        public object Put(OrganizationDto organizationDto)
        {
            var organization = organizationDto.ToEntity();
            Db.Update(organization);

            //Db.Delete<OrganizationOrganizationCategory>(ooc => ooc.OrganizationId == organizationDto.Id);

            //if (organizationDto.Categories != null && organizationDto.Categories.Any())
            //{
            //    Db.InsertAll(organizationDto.Categories.Select(c => new OrganizationOrganizationCategory
            //    {
            //        OrganizationCategoryId = c.Id,
            //        OrganizationId = organizationDto.Id
            //    }));
            //}

            return new OrganizationResponse { Organization = new OrganizationDto() };
        }

        public object Delete(OrganizationDto organizationDto)
        {
            Db.DeleteById<Organization>(organizationDto.Id);
            //Db.Delete<OrganizationOrganizationCategory>(ooc => ooc.OrganizationId == organizationDto.Id);

            return new OrganizationResponse { Organization = new OrganizationDto() };
        }
    }

    /// <summary>
    /// GET /organizations
    /// GET /organizations/category/{CategoryId}
    /// Returns a list of organizations
    /// </summary>
    public class OrganizationsService : ServiceStack.ServiceInterface.Service
    {
        public object Get(OrganizationsDto request)
        {
            var orgs = new List<OrganizationDto>();
            string query;

            if (request.CategoryId.HasValue)
            {
                //var query = string.Format("select o.*, oc.Id as OrganizationCategoryId, oc.Name as OrganizationCategoryName " +
                //                     "from Organization o " +
                //                     "left join OrganizationCategory oc " +
                //                     "on o.OrganizationCategoryId = oc.Id " +
                //                     "where o.OrganizationCategoryId = {0}", request.CategoryId);

                //orgs = Db.Select<OrganizationDto>("select * from Organization");
            }
            else
            {
                query = string.Format("select o.*, sp.Name as StateProvinceName, sp.Abbreviation as StateProvinceAbbreviation, oc.Id as OrganizationCategoryId, oc.Name as OrganizationCategoryName " +
                                     "from Organization o " +
                                     "left join StateProvince sp " +
                                     "on o.StateProvinceId = sp.Id " +
                                     "left join OrganizationCategory oc " +
                                     "on o.OrganizationCategoryId = oc.Id");

                orgs = Db.Select<OrganizationDto>(query);
            }

            foreach (var org in orgs)
            {
                var alliesCountQuery = string.Format("select count(*) from OrganizationAlly where OrganizationId = {0}", org.Id);
                org.OrganizationAlliesCount = Db.Scalar<int>(alliesCountQuery);
            }

            return new OrganizationsResponse
            {
                Organizations = orgs
            };
        }
    }
}