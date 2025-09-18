import {
  Box,
  Typography,
  Button,
  Container,
  Paper,
  Stack,
} from "@mui/material";
import { ErrorOutline, Refresh, ArrowBack } from "@mui/icons-material";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";

export interface ErrorFallbackProps {
  resetErrorBoundary?: () => void;
}
export const ErrorFallback = ({
  resetErrorBoundary,
}: ErrorFallbackProps) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const handleRefresh = () => {
    if (resetErrorBoundary) {
      resetErrorBoundary();
    } else {
      window.location.reload();
    }
  };

  const handleGoBack = () => {
    window.location.href = "/";
  };

  return (
    <Box className={classes.container}>
      <Container maxWidth="md">
        <Paper elevation={3} className={classes.paper}>
          <Box className={classes.contentBox}>
            {/* Error Icon */}
            <Box className={classes.iconContainer}>
              <ErrorOutline className={classes.errorIcon} />
            </Box>

            {/* Main Message */}
            <Typography
              variant="h3"
              component="h1"
              gutterBottom
              className={classes.title}
            >
              {t("errorBoundary.title")}
            </Typography>

            <Typography variant="h6" className={classes.message}>
              {t("errorBoundary.message")}
            </Typography>

            {/* Action Buttons */}
            <Stack
              direction={{ xs: "column", sm: "row" }}
              dir="ltr"
              spacing={2}
              justifyContent="center"
              className={classes.buttonStack}
            >
              <Button
                className={`${classes.baseButton} ${classes.outlinedButton}`}
                variant="outlined"
                size="large"
                onClick={handleGoBack}
                startIcon={<ArrowBack />}
              >
                {t("errorBoundary.goBack")}
              </Button>

              <Button
                className={`${classes.baseButton} ${classes.containedButton}`}
                variant="contained"
                size="large"
                onClick={handleRefresh}
                startIcon={<Refresh />}
              >
                {t("errorBoundary.tryAgain")}
              </Button>
            </Stack>

            {/* Subtle Animation */}
            <Box className={classes.floatingElement} />
          </Box>
        </Paper>
      </Container>
    </Box>
  );
};