// import { useState } from "react";
// import { useAuth } from "@app-providers";
// import {
//   useGetGameHistorySummary,
//   useGetGameHistoryDetailed,
// } from "@student/api";
// import { useTranslation } from "react-i18next";
// import { getDifficultyLabel } from "../practice/utils";
// import { ChevronDown, ChevronUp, Play } from "lucide-react";
// import {
//   Box,
//   Button,
//   Table,
//   TableBody,
//   TableCell,
//   TableContainer,
//   TableHead,
//   TableRow,
//   Paper,
//   Typography,
//   Select,
//   MenuItem,
//   FormControl,
//   InputLabel,
//   Chip,
//   IconButton,
//   Collapse,
//   CircularProgress,
// } from "@mui/material";

// export const PracticeHistory = () => {
//   const { user } = useAuth();
//   const { t, i18n } = useTranslation();
//   const studentId = user?.userId ?? "";

//   const [viewMode, setViewMode] = useState<"summary" | "detailed">("summary");
//   const [expandedGames, setExpandedGames] = useState<Set<string>>(new Set());
//   const [page, setPage] = useState(1);
//   const [pageSize, setPageSize] = useState(10);
//   const isHebrew = i18n.language === "he";

//   // Fetch summary + detailed always (to have latest attempt info for summary)
//   const { data: summaryData, isLoading: summaryLoading } =
//     useGetGameHistorySummary({
//       studentId,
//       page,
//       pageSize,
//     });

//   const { data: detailedData, isLoading: detailedLoading } =
//     useGetGameHistoryDetailed({
//       studentId,
//       page,
//       pageSize,
//     });

//   const isLoading =
//     (viewMode === "summary" && summaryLoading) ||
//     (viewMode === "detailed" && detailedLoading);
//   const currentData = viewMode === "summary" ? summaryData : detailedData;

//   // should be changed
//   if (viewMode === "summary" && !summaryData && !summaryLoading) return null;
//   if (viewMode === "detailed" && !detailedData && !detailedLoading) return null;

//   const toggleExpanded = (gameKey: string) => {
//     setExpandedGames((prev) => {
//       const newSet = new Set(prev);
//       if (newSet.has(gameKey)) {
//         newSet.delete(gameKey);
//       } else {
//         newSet.add(gameKey);
//       }
//       return newSet;
//     });
//   };

//   const getStatusColor = (status: string) => {
//     switch (status) {
//       case "Success":
//         return "success";
//       case "Failure":
//         return "error";
//       case "Pending":
//         return "warning";
//       default:
//         return "default";
//     }
//   };

//   const formatDate = (dateString: string) => {
//     return new Date(dateString).toLocaleString("he-IL", {
//       day: "2-digit",
//       month: "2-digit",
//       hour: "2-digit",
//       minute: "2-digit",
//     });
//   };

//   const renderSummaryView = () => {
//     if (!summaryData) return null;

//     return (
//       <TableContainer component={Paper}>
//         <Table>
//           <TableHead>
//             <TableRow>
//               <TableCell align="right">
//                 {t("pages.practiceHistory.game")}
//               </TableCell>
//               <TableCell align="right">
//                 {t("pages.practiceHistory.yourLastAnswer")}
//               </TableCell>
//               <TableCell align="right">
//                 {t("pages.practiceHistory.status")}
//               </TableCell>
//               <TableCell align="right">
//                 {t("pages.practiceHistory.action")}
//               </TableCell>
//             </TableRow>
//           </TableHead>
//           <TableBody>
//             {summaryData.items.map((item, index) => {
//               console.log("Looking for:", item.gameType, item.difficulty);
//               console.log(
//                 "Available in detailedData:",
//                 detailedData?.items.map((d) => `${d.gameType}|${d.difficulty}`),
//               );
//               const lastAttempt = detailedData?.items
//                 .filter((detail) => {
//                   const match =
//                     detail.gameType === item.gameType &&
//                     detail.difficulty === item.difficulty;
//                   console.log(
//                     `Comparing ${detail.gameType}|${detail.difficulty} with ${item.gameType}|${item.difficulty}: ${match}`,
//                   );
//                   return match;
//                 })
//                 .sort((a, b) => b.attemptNumber - a.attemptNumber)[0];

//               console.log("Item:", item.gameType, item.difficulty);
//               console.log("Last attempt:", lastAttempt);
//               console.log("Given answer:", lastAttempt?.givenAnswer);

//               return (
//                 <TableRow key={index} hover>
//                   {/* Game name + correct answer */}
//                   <TableCell align="right">
//                     <Box>
//                       <Typography variant="body2" fontWeight="medium">
//                         {t(
//                           `pages.practiceHistory.practiceTools.${item.gameType}`,
//                         )}
//                         {lastAttempt?.correctAnswer?.length ? (
//                           <>: "{lastAttempt.correctAnswer.join(" ")}"</>
//                         ) : null}
//                       </Typography>
//                       <Typography variant="caption" color="text.secondary">
//                         {getDifficultyLabel(
//                           Number(item.difficulty) as 0 | 1 | 2,
//                           t,
//                         )}
//                       </Typography>
//                     </Box>
//                   </TableCell>

//                   {/* User's last answer */}
//                   <TableCell align="right">
//                     <Typography variant="body2">
//                       {lastAttempt?.givenAnswer?.length
//                         ? lastAttempt.givenAnswer.join(", ")
//                         : t("pages.practiceHistory.noAttemptYet")}
//                     </Typography>
//                   </TableCell>

//                   {/* Status */}
//                   <TableCell align="right">
//                     <Typography
//                       variant="body2"
//                       color={
//                         item.totalSuccesses > 0
//                           ? "success.main"
//                           : "text.primary"
//                       }
//                     >
//                       {item.totalSuccesses > 0
//                         ? t("pages.practiceHistory.correct")
//                         : t("pages.practiceHistory.tryAgain")}
//                     </Typography>
//                   </TableCell>

//                   {/* Retry button */}
//                   <TableCell align="right">
//                     <Button
//                       sx={{
//                         gap: 1.5,
//                         display: "flex",
//                         justifyContent: "center",
//                         px: 1,
//                         minWidth: "unset",
//                       }}
//                       variant="contained"
//                       size="small"
//                       startIcon={!isHebrew && <Play size={16} />}
//                       endIcon={isHebrew && <Play size={16} />}
//                       onClick={() =>
//                         console.log(
//                           "Retry game:",
//                           item.gameType,
//                           item.difficulty,
//                         )
//                       }
//                     >
//                       <Typography
//                         variant="button"
//                         sx={{
//                           paddingRight: isHebrew ? "5px" : 0,
//                           paddingLeft: !isHebrew ? "10px" : 0,
//                         }}
//                       >
//                         {t("pages.practiceHistory.retry")}
//                       </Typography>
//                     </Button>
//                   </TableCell>
//                 </TableRow>
//               );
//             })}
//           </TableBody>
//         </Table>
//       </TableContainer>
//     );
//   };

//   const renderDetailedView = () => {
//     if (!detailedData) {
//       return (
//         <Paper sx={{ p: 4, textAlign: "center" }}>
//           <Typography variant="body1" color="text.secondary">
//             {t("pages.practiceHistory.noDetailedData")}
//           </Typography>
//         </Paper>
//       );
//     }

//     if (detailedData.items.length === 0) {
//       return (
//         <Paper sx={{ p: 4, textAlign: "center" }}>
//           <Typography variant="body1" color="text.secondary">
//             {t("pages.practiceHistory.noAttemptsYet")}
//           </Typography>
//         </Paper>
//       );
//     }

//     // Group attempts by game + difficulty
//     const groupedAttempts = detailedData.items.reduce(
//       (acc, item) => {
//         const key = `${item.gameType}-${item.difficulty}`;
//         if (!acc[key]) {
//           acc[key] = {
//             gameType: item.gameType,
//             difficulty: item.difficulty,
//             attempts: [],
//           };
//         }
//         acc[key].attempts.push(item);
//         return acc;
//       },
//       {} as Record<
//         string,
//         {
//           gameType: string;
//           difficulty: string;
//           attempts: typeof detailedData.items;
//         }
//       >,
//     );

//     return (
//       <Box>
//         {Object.entries(groupedAttempts).map(([key, group]) => {
//           const isExpanded = expandedGames.has(key);

//           return (
//             <Paper key={key} sx={{ mb: 2 }}>
//               <Box
//                 onClick={() => toggleExpanded(key)}
//                 sx={{
//                   p: 2,
//                   display: "flex",
//                   alignItems: "center",
//                   justifyContent: "space-between",
//                   cursor: "pointer",
//                   "&:hover": { bgcolor: "action.hover" },
//                 }}
//               >
//                 <Box display="flex" alignItems="center" gap={2}>
//                   <Box>
//                     <Typography variant="h6">
//                       {t(
//                         `pages.practiceHistory.practiceTools.${group.gameType}`,
//                       )}
//                     </Typography>
//                     <Typography variant="caption" color="text.secondary">
//                       {getDifficultyLabel(
//                         Number(group.difficulty) as 0 | 1 | 2,
//                         t,
//                       )}
//                     </Typography>
//                   </Box>
//                   <Typography variant="body2" color="text.secondary">
//                     {t("pages.practiceHistory.attemptsCount", {
//                       count: group.attempts.length,
//                     })}
//                   </Typography>
//                 </Box>
//                 <IconButton size="small">
//                   {isExpanded ? <ChevronUp /> : <ChevronDown />}
//                 </IconButton>
//               </Box>

//               <Collapse in={isExpanded}>
//                 <Box sx={{ borderTop: 1, borderColor: "divider" }}>
//                   <Table size="small">
//                     <TableHead>
//                       <TableRow>
//                         <TableCell align="right">
//                           {t("pages.practiceHistory.attemptNumber")}
//                         </TableCell>
//                         <TableCell align="right">
//                           {t("pages.practiceHistory.answer")}
//                         </TableCell>
//                         <TableCell align="right">
//                           {t("pages.practiceHistory.result")}
//                         </TableCell>
//                         <TableCell align="right">
//                           {t("pages.practiceHistory.timestamp")}
//                         </TableCell>
//                       </TableRow>
//                     </TableHead>
//                     <TableBody>
//                       {group.attempts.map((attempt) => (
//                         <TableRow key={attempt.attemptId}>
//                           <TableCell align="right">
//                             {attempt.attemptNumber}
//                           </TableCell>
//                           <TableCell align="right" dir="rtl">
//                             {attempt.givenAnswer.join(", ")}
//                           </TableCell>
//                           <TableCell align="right">
//                             <Chip
//                               label={
//                                 attempt.status === "Success"
//                                   ? t("pages.practiceHistory.correct")
//                                   : t("pages.practiceHistory.tryAgain")
//                               }
//                               color={
//                                 getStatusColor(attempt.status) as
//                                   | "success"
//                                   | "error"
//                                   | "warning"
//                                   | "default"
//                               }
//                               size="small"
//                             />
//                           </TableCell>
//                           <TableCell align="right">
//                             {formatDate(attempt.createdAt)}
//                           </TableCell>
//                         </TableRow>
//                       ))}
//                     </TableBody>
//                   </Table>
//                 </Box>
//               </Collapse>
//             </Paper>
//           );
//         })}
//       </Box>
//     );
//   };

//   return (
//     <Box sx={{ maxWidth: "1200px", mx: "auto", p: 3 }} dir="rtl">
//       <Box mb={3}>
//         <Typography variant="h4" component="h1" gutterBottom>
//           {t("pages.practiceHistory.title")}
//         </Typography>
//         <Typography variant="body2" color="text.secondary">
//           {t("pages.practiceHistory.subtitle")}
//         </Typography>
//       </Box>

//       <Box
//         display="flex"
//         justifyContent="space-between"
//         alignItems="center"
//         mb={3}
//       >
//         <Box display="flex" gap={1}>
//           <Button
//             variant={viewMode === "summary" ? "contained" : "outlined"}
//             onClick={() => setViewMode("summary")}
//           >
//             {t("pages.practiceHistory.summaryMode")}
//           </Button>
//           <Button
//             variant={viewMode === "detailed" ? "contained" : "outlined"}
//             onClick={() => setViewMode("detailed")}
//           >
//             {t("pages.practiceHistory.detailedMode")}
//           </Button>
//         </Box>

//         <FormControl size="small" sx={{ minWidth: 120 }}>
//           <InputLabel>{t("pages.practiceHistory.perPage")}</InputLabel>
//           <Select
//             value={pageSize}
//             label={t("pages.practiceHistory.perPage")}
//             onChange={(e) => setPageSize(Number(e.target.value))}
//           >
//             <MenuItem value={5}>5</MenuItem>
//             <MenuItem value={10}>10</MenuItem>
//             <MenuItem value={20}>20</MenuItem>
//           </Select>
//         </FormControl>
//       </Box>

//       {isLoading ? (
//         <Box display="flex" justifyContent="center" py={6}>
//           <CircularProgress />
//         </Box>
//       ) : (
//         <>
//           {viewMode === "summary" ? renderSummaryView() : renderDetailedView()}

//           {currentData && currentData.totalCount > pageSize && (
//             <Box
//               display="flex"
//               justifyContent="space-between"
//               alignItems="center"
//               mt={3}
//             >
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

import { useState } from "react";
import { useAuth } from "@app-providers";
import {
  useGetGameHistorySummary,
  useGetGameHistoryDetailed,
} from "@student/api";
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
import { SummaryView } from "./components/summary-view";
import { DetailedView } from "./components/detailed-view";

export const PracticeHistory = () => {
  const { user } = useAuth();
  const { t, i18n } = useTranslation();
  const studentId = user?.userId ?? "";

  const [viewMode, setViewMode] = useState<"summary" | "detailed">("summary");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const isHebrew = i18n.language === "he";

  const { data: summaryData, isLoading: summaryLoading } =
    useGetGameHistorySummary({
      studentId,
      page,
      pageSize,
    });

  const { data: detailedData, isLoading: detailedLoading } =
    useGetGameHistoryDetailed({
      studentId,
      page,
      pageSize: 9999,
    });

  const isLoading =
    (viewMode === "summary" && (summaryLoading || detailedLoading)) ||
    (viewMode === "detailed" && detailedLoading);

  const currentData = viewMode === "summary" ? summaryData : detailedData;

  if (viewMode === "summary" && !summaryData && !summaryLoading) return null;
  if (viewMode === "detailed" && !detailedData && !detailedLoading)
    return null;

  return (
    <Box sx={{ maxWidth: "1200px", mx: "auto", p: 3 }} dir="rtl">
      <Box mb={3}>
        <Typography variant="h4" component="h1" gutterBottom>
          {t("pages.practiceHistory.title")}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t("pages.practiceHistory.subtitle")}
        </Typography>
      </Box>

      <Box
        display="flex"
        justifyContent="space-between"
        alignItems="center"
        mb={3}
      >
        <Box display="flex" gap={1}>
          <Button
            variant={viewMode === "summary" ? "contained" : "outlined"}
            onClick={() => setViewMode("summary")}
          >
            {t("pages.practiceHistory.summaryMode")}
          </Button>
          <Button
            variant={viewMode === "detailed" ? "contained" : "outlined"}
            onClick={() => setViewMode("detailed")}
          >
            {t("pages.practiceHistory.detailedMode")}
          </Button>
        </Box>

        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>{t("pages.practiceHistory.perPage")}</InputLabel>
          <Select
            value={pageSize}
            label={t("pages.practiceHistory.perPage")}
            onChange={(e) => setPageSize(Number(e.target.value))}
          >
            <MenuItem value={5}>5</MenuItem>
            <MenuItem value={10}>10</MenuItem>
            <MenuItem value={20}>20</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {isLoading ? (
        <Box display="flex" justifyContent="center" py={6}>
          <CircularProgress />
        </Box>
      ) : (
        <>
          {viewMode === "summary" ? (
            <SummaryView
              summaryData={summaryData}
              detailedData={detailedData}
              isHebrew={isHebrew}
            />
          ) : (
            <DetailedView detailedData={detailedData} />
          )}

          {currentData && currentData.totalCount > pageSize && (
            <Box
              display="flex"
              justifyContent="space-between"
              alignItems="center"
              mt={3}
            >
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