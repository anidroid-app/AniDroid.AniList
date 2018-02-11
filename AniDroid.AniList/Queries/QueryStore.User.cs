﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AniDroid.AniList.Queries
{
    internal static partial class QueryStore
    {
        /// <summary>
        /// Parameters: (name: string)
        /// <para></para>
        /// Returns: User
        /// </summary>
        public static string GetUserByName => @"
query ($name: String) {
  Data: User(name: $name) {
    id
    name
    about(asHtml: true)
    avatar {
      large
      medium
    }
    bannerImage
    isFollowing
    options {
      titleLanguage
      displayAdultContent
    }
    mediaListOptions {
      scoreFormat
      rowOrder
      useLegacyLists
    }
    stats {
      watchedTime
      chaptersRead
      activityHistory {
        date
        amount
        level
      }
      animeStatusDistribution {
        status
        amount
      }
      mangaStatusDistribution {
        status
        amount
      }
      animeScoreDistribution {
        score
        amount
      }
      mangaScoreDistribution {
        score
        amount
      }
    }
    donatorTier
    unreadNotificationCount
    siteUrl
    updatedAt
  }
}

";

        /// <summary>
        /// Parameters: (name: string, type: MediaType)
        /// <para></para>
        /// Returns: MediaListCollection
        /// </summary>
        public static string GetMediaListsByUserNameAndType => @"
query ($name: String, $type: MediaType) {
  Data: MediaListCollection(userName: $name, type: $type) {
    user {
      name
      mediaListOptions {
        scoreFormat
        animeList {
          customLists
          sectionOrder
        }
        mangaList {
          customLists
          sectionOrder
        }
      }
    }
    statusLists(asArray: true) {
      status
      score
      progress
      progressVolumes
      repeat
      priority
      notes
      hiddenFromStatusLists
      startedAt {
        year
        month
        day
      }
      completedAt {
        year
        month
        day
      }
      customLists
      media {
        id
        title {
          userPreferred
          english
          romaji
          native
        }
        coverImage {
          medium
          large
        }
        status
        episodes
        chapters
        volumes
        format
        averageScore
        meanScore
        popularity
        startDate {
          year
          month
          day
        }
        endDate {
          year
          month
          day
        }
      }
    }
  }
}
";

        /// <summary>
        /// Parameters: (queryText: string, page: int, count: int)
        /// <para></para>
        /// Returns: PagedData of User
        /// </summary>
        public static string SearchUsers => @"
query ($queryText: String, $page: Int, $count: Int) {
  Data: Page(page: $page, perPage: $count) {
    pageInfo {
      total
      perPage
      currentPage
      lastPage
      hasNextPage
    }
    Data: users(search: $queryText) {
      id
      name
      avatar {
        large
      }
      isFollowing
    }
  }
}
";
    }
}