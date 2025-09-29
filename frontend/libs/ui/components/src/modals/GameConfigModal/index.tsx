import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  FormGroup,
  Checkbox,
  TextField,
  Button,
  Typography,
  Box,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { DifficultyLevel } from "@student/types";
import { useStyles } from "./style";

export interface GameConfig {
  difficulty: DifficultyLevel;
  nikud: boolean;
  count: number;
}

interface GameConfigModalProps {
  open: boolean;
  onClose: () => void;
  onConfirm: (config: GameConfig) => void;
  getDifficultyLevelLabel: (
    level: DifficultyLevel,
    t: (key: string) => string,
  ) => string;
  initialConfig?: GameConfig;
}

export const GameConfigModal = ({
  open,
  onClose,
  onConfirm,
  getDifficultyLevelLabel,
  initialConfig = {
    difficulty: 1,
    nikud: false,
    count: 3,
  },
}: GameConfigModalProps) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const [config, setConfig] = useState<GameConfig>(initialConfig);

  const handleDifficultyChange = (
    event: React.ChangeEvent<HTMLInputElement>,
  ) => {
    setConfig((prev) => ({
      ...prev,
      difficulty: parseInt(event.target.value) as DifficultyLevel,
    }));
  };

  const handleNikudChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setConfig((prev) => ({
      ...prev,
      nikud: event.target.checked,
    }));
  };

  const handleCountChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(event.target.value);
    if (value >= 1 && value <= 10) {
      setConfig((prev) => ({
        ...prev,
        count: value,
      }));
    }
  };

  const handleConfirm = () => {
    onConfirm(config);
    onClose();
  };

  return (
    <Dialog
      className={classes.gameConfigModal}
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
    >
      <DialogTitle>
        <Typography variant="h5" component="div">
          {t("pages.wordOrderGame.config.title")}
        </Typography>
        <Typography
          variant="body2"
          color="text.secondary"
          className={classes.modalTitle}
        >
          {t("pages.wordOrderGame.config.subtitle")}
        </Typography>
      </DialogTitle>

      <DialogContent data-testid="typing-level-selection">
        <Box className={classes.modalContent}>
          {/* Difficulty Selection */}
          <FormControl component="fieldset">
            <FormLabel component="legend" className={classes.formLabel}>
              <Typography variant="subtitle1" fontWeight="medium">
                {t("pages.wordOrderGame.config.difficulty")}
              </Typography>
            </FormLabel>
            <RadioGroup
              value={config.difficulty.toString()}
              onChange={handleDifficultyChange}
            >
              <FormControlLabel
                value="0"
                control={<Radio data-testid="typing-level-easy" />}
                label={
                  <Box>
                    <Typography variant="body1">
                      {getDifficultyLevelLabel(0, t)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {t("pages.wordOrderGame.config.difficultyDesc.easy")}
                    </Typography>
                  </Box>
                }
              />
              <FormControlLabel
                value="1"
                control={<Radio data-testid="typing-level-medium" />}
                label={
                  <Box>
                    <Typography variant="body1">
                      {getDifficultyLevelLabel(1, t)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {t("pages.wordOrderGame.config.difficultyDesc.medium")}
                    </Typography>
                  </Box>
                }
              />
              <FormControlLabel
                value="2"
                control={<Radio data-testid="typing-level-hard" />}
                label={
                  <Box>
                    <Typography variant="body1">
                      {getDifficultyLevelLabel(2, t)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {t("pages.wordOrderGame.config.difficultyDesc.hard")}
                    </Typography>
                  </Box>
                }
              />
            </RadioGroup>
          </FormControl>

          {/* Nikud Option */}
          <FormControl component="fieldset">
            <FormLabel component="legend" className={classes.formLabel}>
              <Typography variant="subtitle1" fontWeight="medium">
                {t("pages.wordOrderGame.config.options")}
              </Typography>
            </FormLabel>
            <FormGroup>
              <FormControlLabel
                data-testid="game-config-nikud"
                control={
                  <Checkbox
                    checked={config.nikud}
                    onChange={handleNikudChange}
                  />
                }
                label={
                  <Box>
                    <Typography variant="body1">
                      {t("pages.wordOrderGame.config.nikud")}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {t("pages.wordOrderGame.config.nikudDesc")}
                    </Typography>
                  </Box>
                }
              />
            </FormGroup>
          </FormControl>

          {/* Sentence Count */}
          <FormControl>
            <FormLabel className={classes.formLabel}>
              <Typography variant="subtitle1" fontWeight="medium">
                {t("pages.wordOrderGame.config.sentenceCount")}
              </Typography>
            </FormLabel>
            <TextField
              type="number"
              value={config.count}
              onChange={handleCountChange}
              helperText={t("pages.wordOrderGame.config.sentenceCountDesc")}
              size="small"
              className={classes.sentenceCountField}
            />
          </FormControl>
        </Box>
      </DialogContent>

      <DialogActions className={classes.modalActions}>
        <Button onClick={onClose} color="inherit">
          {t("pages.wordOrderGame.cancel")}
        </Button>
        <Button
          onClick={handleConfirm}
          variant="contained"
          color="primary"
          className={classes.startGameButton}
          data-testid="game-config-start"
        >
          {t("pages.wordOrderGame.config.startGame")}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
