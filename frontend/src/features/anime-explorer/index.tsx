import { useEffect, useMemo, useRef, useState } from "react";

import { useTranslation } from "react-i18next";

import { useGetAnimeSearch } from "./api";
import { AnimeCard, Header } from "./components";
import { useDebounceValue } from "./utils";

import { useStyles } from "./style";

export const AnimeExplorer = () => {
  const { t } = useTranslation();
  const [search, setSearch] = useState("");
  const debouncedSearch = useDebounceValue(search, 600);
  const loadMoreRef = useRef<HTMLDivElement>(null);
  const listRef = useRef<HTMLDivElement>(null);
  const [showBackToTop, setShowBackToTop] = useState(false);

  const {
    data,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
  } = useGetAnimeSearch({ search: debouncedSearch });

  const classes = useStyles({ isFetchingNextPage, showBackToTop });

  useEffect(
    function loadMore() {
      if (!loadMoreRef.current || !hasNextPage || isFetchingNextPage) return;
      const observer = new IntersectionObserver(
        ([entry]) => entry.isIntersecting && fetchNextPage(),
        { root: listRef.current ?? null, rootMargin: "300px" },
      );
      observer.observe(loadMoreRef.current);
      return () => observer.disconnect();
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage],
  );

  useEffect(
    function scrollToTopOnSearch() {
      listRef.current?.scrollTo({ top: 0, behavior: "smooth" });
    },
    [debouncedSearch],
  );

  const handleListScroll = () => {
    const element = listRef.current;
    if (!element) return;
    const threshold = Math.max(300, element.clientHeight * 0.5);
    setShowBackToTop(element.scrollTop > threshold);
  };

  const dataList = useMemo(
    () => data?.pages.flatMap((page) => page.data) || [],
    [data],
  );

  if (error) return <p>Error fetching anime data: {error.message}</p>;

  return (
    <div className={classes.root}>
      <Header initial={search} onDebouncedChange={setSearch} />
      <div
        ref={listRef}
        className={classes.listWrap}
        onScroll={handleListScroll}
      >
        <div className={classes.listInner}>
          <div className={classes.grid}>
            {!isLoading &&
              dataList.length > 0 &&
              dataList.map((anime) => (
                <AnimeCard key={anime.mal_id} anime={anime} />
              ))}
          </div>

          {isLoading && (
            <p className={classes.loading}>
              {t("pages.animeExplorer.loading")}
            </p>
          )}
          {hasNextPage && (
            <div ref={loadMoreRef} className={classes.sentinel} />
          )}

          <p
            className={classes.loadingMore}
            aria-live="polite"
            aria-atomic="true"
          >
            {t("pages.animeExplorer.loadingMore")}
          </p>
          <span className={classes.srOnly} role="status">
            {isFetchingNextPage
              ? t("pages.animeExplorer.loadingMoreResults")
              : ""}
          </span>

          {!hasNextPage && !!dataList.length && (
            <p style={{ textAlign: "center", color: "#334155", marginTop: 16 }}>
              {t("pages.animeExplorer.reachEnd")}
            </p>
          )}
        </div>
      </div>

      <button
        className={classes.backToTop}
        onClick={() =>
          listRef.current?.scrollTo({ top: 0, behavior: "smooth" })
        }
        aria-label="Back to top"
      >
        ↑ {t("pages.animeExplorer.top")}
      </button>
    </div>
  );
};
