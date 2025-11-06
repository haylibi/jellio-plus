import { useEffect, useState } from 'react';
import useAccessToken from '@/hooks/useAccessToken.ts';
import { getServerInfo } from '@/services/backendService.ts';
import type { ServerInfo, Maybe } from '@/types';

const useServerInfo = (): Maybe<ServerInfo> => {
  const accessToken = useAccessToken();
  const [serverInfo, setServerInfo] = useState<ServerInfo | null | undefined>();

  useEffect(() => {
    const fetchServerInfo = async (): Promise<void> => {
      try {
        // Try to fetch using session auth (cookies)
        var serverInfo = await getServerInfo();
        setServerInfo({
          accessToken: accessToken || '', // May not exist, that's OK
          ...serverInfo,
        });
      } catch (error) {
        console.error('Failed to authenticate with Jellyfin session:', error);
        setServerInfo(null);
      }
    };
    
    void fetchServerInfo();
  }, [accessToken]);

  return serverInfo;
};

export default useServerInfo;
