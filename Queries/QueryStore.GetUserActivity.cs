﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AniDroid.AniList.Queries
{
    internal static partial class QueryStore
    {
        public const string GetUserActivity = @"
query {
  Data: Page {
    pageInfo {
      total
      perPage
      currentPage
      lastPage
      hasNextPage
    }
    Data: activities(isFollowing: true, sort: ID_DESC) {
      ... on TextActivity {
        id
        userId
        type
        replyCount
        text
        createdAt
        user {
          id
          name
          avatar {
            large
          }
        }
        likes {
          id
          name
          avatar {
            large
          }
        }
        replies {
          id
          text
          createdAt
          user {
            id
            name
            avatar {
              large
            }
          }
          likes {
            id
            name
            avatar {
              large
            }
          }
        }
      }
      ... on ListActivity {
        id
        userId
        type
        status
        progress
        replyCount
        createdAt
        user {
          id
          name
          avatar {
            large
          }
        }
        media {
          id
          title {
            userPreferred
          }
        }
        likes {
          id
          name
          avatar {
            large
          }
        }
        replies {
          id
          text
          createdAt
          user {
            id
            name
            avatar {
              large
            }
          }
          likes {
            id
            name
            avatar {
              large
            }
          }
        }
      }
      ... on MessageActivity {
        id
        type
        replyCount
        createdAt
        messenger {
          id
          name
          avatar {
            large
          }
        }
        likes {
          id
          name
          avatar {
            large
          }
        }
        replies {
          id
          text
          createdAt
          user {
            id
            name
            avatar {
              large
            }
          }
          likes {
            id
            name
            avatar {
              large
            }
          }
        }
      }
    }
  }
}
";
    }
}
