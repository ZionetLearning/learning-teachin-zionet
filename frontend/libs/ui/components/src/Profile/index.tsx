import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Typography, TextField, Stack } from '@mui/material';
import { useUpdateUser, decodeJwtUserId, useAuth } from '@app-providers';
import { Button } from '../Button';

export type ProfileProps = {
    firstName: string;
    lastName: string;
    email: string;
};

export const Profile = ({
    firstName,
    lastName,
    email,

}: ProfileProps) => {
    const { t, i18n } = useTranslation();
    const { mutateAsync: updateUserMutation } = useUpdateUser();
    const { user, setUser } = useAuth();
    const [fn, setFn] = useState(firstName);
    const [ln, setLn] = useState(lastName);

    const isRTL = i18n.dir() === 'rtl';

    useEffect(() => {
        setFn(firstName);
        setLn(lastName);
    }, [firstName, lastName]);

    const dirty =
        fn.trim() !== firstName.trim() || ln.trim() !== lastName.trim();


    const raw = localStorage.getItem("credentials");
    const creds = raw ? JSON.parse(raw) : null;
    const userId = decodeJwtUserId(creds.accessToken);
    if (!userId) {
        console.error("User ID not found in token");
    }

    const handleCancel = () => {
        setFn(firstName);
        setLn(lastName);
    };

    const handleSave = async () => {
        if (!user) return;
        try {
            if (!userId) throw new Error("Missing user ID");
            await updateUserMutation({
                userId,
                firstName: fn,
                lastName: ln,
            });
            setUser?.({ ...user, firstName: fn, lastName: ln });
        } catch (err) {
            console.error("Update failed:", err);
        }
    };

    return (
        <Box
            sx={{
                minHeight: '100dvh',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'center',
                p: 2,
                '@media (min-width: 600px)': {
                    px: 4,
                },
                '@media (min-width: 900px)': {
                    px: 8,
                },
                '@media (min-width: 1200px)': {
                    px: 40,
                }
            }}
        >
            <Box sx={{ mb: 2 }}>
                <Typography variant="h4" fontWeight={700}>
                    {t('pages.profile.title')}
                </Typography>
            </Box>

            <Box
                sx={{
                    border: '1px solid',
                    borderColor: 'divider',
                    borderRadius: 3,
                    p: 3,
                    bgcolor: 'background.paper',
                    maxWidth: 600,
                    mx: 'auto',
                    width: '100%'
                }}
            >
                <Box sx={{ mb: 3 }}>
                    <Typography variant="h6">
                        {t('pages.profile.subTitle')}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                        {t('pages.profile.secondSubTitle')}
                    </Typography>
                </Box>

                <Stack spacing={3}>
                    <Box>
                        <Typography
                            variant="body2"
                            color="text.primary"
                            sx={{
                                mb: 0.3,
                                textAlign: isRTL ? 'right' : 'left',
                                fontWeight: 300
                            }}
                        >
                            {t('pages.profile.firstName')}
                        </Typography>
                        <TextField
                            value={fn}
                            onChange={(e) => {
                                setFn(e.target.value);
                            }}
                            fullWidth
                            sx={{
                                '& .MuiInputLabel-root': { display: 'none' },
                                '& .MuiInputBase-root': {
                                    direction: isRTL ? 'rtl' : 'ltr',
                                },
                                '&.Mui-disabled': { color: 'text.disabled' },
                                '&.Mui-error': { color: 'error.main' },
                            }}
                        />
                    </Box>

                    <Box>
                        <Typography
                            variant="body2"
                            color="text.primary"
                            sx={{
                                mb: 0.3,
                                textAlign: isRTL ? 'right' : 'left',
                                fontWeight: 300
                            }}
                        >
                            {t('pages.profile.lastName')}
                        </Typography>
                        <TextField
                            value={ln}
                            onChange={(e) => {
                                setLn(e.target.value);
                            }}
                            fullWidth
                            sx={{
                                '& .MuiInputLabel-root': { display: 'none' },
                                '& .MuiInputBase-root': {
                                    direction: isRTL ? 'rtl' : 'ltr',
                                },
                                '&.Mui-disabled': { color: 'text.disabled' },
                                '&.Mui-error': { color: 'error.main' },
                            }}
                        />
                    </Box>

                    <Box>
                        <Typography
                            variant="body2"
                            color="text.primary"
                            sx={{
                                mb: 0.3,
                                textAlign: isRTL ? 'right' : 'left',
                                fontWeight: 100
                            }}
                        >
                            {t('pages.profile.email')}
                        </Typography>
                        <TextField
                            value={email}
                            disabled
                            fullWidth
                            sx={{
                                '& .MuiInputLabel-root': { display: 'none' },
                                '& .MuiInputBase-root': {
                                    direction: isRTL ? 'rtl' : 'ltr',
                                },
                                '&.Mui-disabled': { color: 'text.disabled' },
                                '&.Mui-error': { color: 'error.main' },
                            }}
                        />
                        <Typography
                            variant="body2"
                            color="text.disabled"
                            sx={{
                                mt: 0.5,
                                textAlign: isRTL ? 'right' : 'left',
                                fontWeight: 100,
                                fontSize: '0.75rem'
                            }}
                        >
                            {t('pages.profile.emailCannotBeChanged')}
                        </Typography>
                    </Box>
                </Stack>

                <Box sx={{ mt: 3, display: 'flex', gap: 1 }}>
                    <>
                        <Button
                            onClick={handleSave}
                            disabled={!dirty}
                        >
                            {t('pages.profile.saveChanges')}
                        </Button>
                        <Button variant="outlined" disabled={!dirty} onClick={handleCancel}>
                            {t('pages.profile.cancel')}
                        </Button>
                    </>

                </Box>
            </Box>
        </Box>
    );
};