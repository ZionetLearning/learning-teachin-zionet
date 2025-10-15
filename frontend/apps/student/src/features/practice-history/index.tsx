// import { useState } from "react";
// import { useAuth } from "@app-providers";
// import { useTranslation } from "react-i18next";
// import {
//   Box,
//   Button,
//   Typography,
//   Select,
//   MenuItem,
//   FormControl,
//   InputLabel,
//   CircularProgress,
// } from "@mui/material";
// import {
//   useGetGameHistorySummary,
//   useGetGameHistoryDetailed,
// } from "@student/api";
// import { SummaryView } from "./components/summary-view";
// import { DetailedView } from "./components/detailed-view";
// import { useStyles } from "./style";

// export const PracticeHistory = () => {
//   const { user } = useAuth();
//   const { t, i18n } = useTranslation();
//   const studentId = user?.userId ?? "";

//   const [viewMode, setViewMode] = useState<"summary" | "detailed">("summary");
//   const [page, setPage] = useState(1);
//   const [pageSize, setPageSize] = useState(10);
//   const classes = useStyles();
//   const isHebrew = i18n.language === "he";

//   const { data: summaryData, isLoading: summaryLoading } =
//     useGetGameHistorySummary({
//       studentId,
//       page,
//       pageSize,
//     });

//   const { data: detailedData, isLoading: detailedLoading } =
//     useGetGameHistoryDetailed({
//       studentId,
//       page: viewMode === "detailed" ? page : 1, // Conditional page
//       pageSize: viewMode === "detailed" ? pageSize : 9999, // Conditional pageSize
//     });

//   const isLoading =
//     (viewMode === "summary" && (summaryLoading || detailedLoading)) ||
//     (viewMode === "detailed" && detailedLoading);

//   const currentData = viewMode === "summary" ? summaryData : detailedData;

//   if (viewMode === "summary" && !summaryData && !summaryLoading) return null;
//   if (viewMode === "detailed" && !detailedData && !detailedLoading) return null;

//   return (
//     <Box className={classes.container} dir="rtl">
//       <Box mb={3}>
//         <Typography variant="h4" component="h1" gutterBottom>
//           {t("pages.practiceHistory.title")}
//         </Typography>
//         <Typography variant="body2" color="text.secondary">
//           {t("pages.practiceHistory.subtitle")}
//         </Typography>
//       </Box>

//       <Box className={classes.buttonsContainer}>
//         <Box className={classes.innerButtonsContainer}>
//           <Button
//             variant={viewMode === "summary" ? "contained" : "outlined"}
//             onClick={() => {
//               setViewMode("summary");
//               setPage(1); // Reset to page 1 when switching modes
//             }}
//           >
//             {t("pages.practiceHistory.summaryMode")}
//           </Button>
//           <Button
//             variant={viewMode === "detailed" ? "contained" : "outlined"}
//             onClick={() => {
//               setViewMode("detailed");
//               setPage(1); // Reset to page 1 when switching modes
//             }}
//           >
//             {t("pages.practiceHistory.detailedMode")}
//           </Button>
//         </Box>

//         <FormControl size="small" className={classes.formControl}>
//           <InputLabel>{t("pages.practiceHistory.perPage")}</InputLabel>
//           <Select
//             value={pageSize}
//             label={t("pages.practiceHistory.perPage")}
//             onChange={(e) => {
//               setPageSize(Number(e.target.value));
//               setPage(1); // Reset to page 1 when changing page size
//             }}
//           >
//             <MenuItem value={5}>5</MenuItem>
//             <MenuItem value={10}>10</MenuItem>
//             <MenuItem value={20}>20</MenuItem>
//           </Select>
//         </FormControl>
//       </Box>

//       {isLoading ? (
//         <Box className={classes.loadingContainer}>
//           <CircularProgress />
//         </Box>
//       ) : (
//         <>
//           {viewMode === "summary" ? (
//             <SummaryView
//               summaryData={summaryData}
//               detailedData={detailedData}
//               isHebrew={isHebrew}
//             />
//           ) : (
//             <DetailedView detailedData={detailedData} />
//           )}

//           {currentData && currentData.totalCount > pageSize && (
//             <Box className={classes.navigationButtons}>
//               <Button
//                 onClick={() => setPage((p) => Math.max(1, p - 1))}
//                 disabled={page === 1}
//               >
//                 {t("pages.practiceHistory.previous")}
//               </Button>

//               <Typography variant="body2">
//                 {t("pages.practiceHistory.pageInfo", {
//                   current: page,
//                   total: Math.ceil(currentData.totalCount / pageSize),
//                 })}
//               </Typography>

//               <Button
//                 onClick={() => setPage((p) => p + 1)}
//                 disabled={!currentData.hasNextPage}
//               >
//                 {t("pages.practiceHistory.next")}
//               </Button>
//             </Box>
//           )}
//         </>
//       )}
//     </Box>
//   );
// };

import { useState, useMemo } from "react";
import { useAuth } from "@app-providers";
import { useTranslation } from "react-i18next";
import {
  Box,
  Button,
  Typography,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  CircularProgress,
} from "@mui/material";
import {
  useGetGameHistorySummary,
  useGetAllGameHistoryDetailed,
} from "@student/api";
import { SummaryView } from "./components/summary-view";
import { DetailedView } from "./components/detailed-view";
import { useStyles } from "./style";

export const PracticeHistory = () => {
  const { user } = useAuth();
  const { t, i18n } = useTranslation();
  const studentId = user?.userId ?? "";

  const [viewMode, setViewMode] = useState<"summary" | "detailed">("summary");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const classes = useStyles();
  const isHebrew = i18n.language === "he";

  const { data: summaryData, isLoading: summaryLoading } =
    useGetGameHistorySummary({
      studentId,
      page,
      pageSize,
    });

  console.log("Summary Data:", summaryData); 
  console.log("Summary items count:", summaryData?.items.length);
  console.log("Summary total count:", summaryData?.totalCount);

  // Fetch ALL detailed items at once
  const { data: allDetailedItems, isLoading: allDetailedLoading } =
    useGetAllGameHistoryDetailed(studentId);

  console.log("All detailed items:", allDetailedItems);

  // Manually paginate the detailed items for detailed view
  const paginatedDetailedData = useMemo(() => {
    if (!allDetailedItems) return undefined;

    const startIndex = (page - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedItems = allDetailedItems.slice(startIndex, endIndex);

    return {
      items: paginatedItems,
      page: page,
      pageSize: pageSize,
      totalCount: allDetailedItems.length,
      hasNextPage: endIndex < allDetailedItems.length,
    };
  }, [allDetailedItems, page, pageSize]);

  // Create full detailed data structure for summary view
  const allDetailedData = useMemo(() => {
    if (!allDetailedItems) return undefined;

    return {
      items: allDetailedItems,
      page: 1,
      pageSize: allDetailedItems.length,
      totalCount: allDetailedItems.length,
      hasNextPage: false,
    };
  }, [allDetailedItems]);

  const isLoading = summaryLoading || allDetailedLoading;

  // Determine which data to use for pagination controls
  const currentData =
    viewMode === "summary" ? summaryData : paginatedDetailedData;

  // Determine which detailed data to pass to views
  const detailedDataForView =
    viewMode === "summary" ? allDetailedData : paginatedDetailedData;

  if (viewMode === "summary" && !summaryData && !summaryLoading) return null;
  if (viewMode === "detailed" && !allDetailedItems && !allDetailedLoading)
    return null;

  return (
    <Box className={classes.container} dir="rtl">
      <Box mb={3}>
        <Typography variant="h4" component="h1" gutterBottom>
          {t("pages.practiceHistory.title")}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t("pages.practiceHistory.subtitle")}
        </Typography>
      </Box>

      <Box className={classes.buttonsContainer}>
        <Box className={classes.innerButtonsContainer}>
          <Button
            variant={viewMode === "summary" ? "contained" : "outlined"}
            onClick={() => {
              setViewMode("summary");
              setPage(1);
            }}
          >
            {t("pages.practiceHistory.summaryMode")}
          </Button>
          <Button
            variant={viewMode === "detailed" ? "contained" : "outlined"}
            onClick={() => {
              setViewMode("detailed");
              setPage(1);
            }}
          >
            {t("pages.practiceHistory.detailedMode")}
          </Button>
        </Box>

        <FormControl size="small" className={classes.formControl}>
          <InputLabel>{t("pages.practiceHistory.perPage")}</InputLabel>
          <Select
            value={pageSize}
            label={t("pages.practiceHistory.perPage")}
            onChange={(e) => {
              setPageSize(Number(e.target.value));
              setPage(1);
            }}
          >
            <MenuItem value={5}>5</MenuItem>
            <MenuItem value={10}>10</MenuItem>
            <MenuItem value={20}>20</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {isLoading ? (
        <Box className={classes.loadingContainer}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          {viewMode === "summary" ? (
            <SummaryView
              summaryData={summaryData}
              detailedData={detailedDataForView} // All detailed items
              isHebrew={isHebrew}
            />
          ) : (
            <DetailedView detailedData={detailedDataForView} /> // Paginated detailed items
          )}

          {currentData && currentData.totalCount > pageSize && (
            <Box className={classes.navigationButtons}>
              <Button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
              >
                {t("pages.practiceHistory.previous")}
              </Button>

              <Typography variant="body2">
                {t("pages.practiceHistory.pageInfo", {
                  current: page,
                  total: Math.ceil(currentData.totalCount / pageSize),
                })}
              </Typography>

              <Button
                onClick={() => setPage((p) => p + 1)}
                disabled={!currentData.hasNextPage}
              >
                {t("pages.practiceHistory.next")}
              </Button>
            </Box>
          )}
        </>
      )}
    </Box>
  );
};
