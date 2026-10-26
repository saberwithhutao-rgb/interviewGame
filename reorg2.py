import os,shutil
A='Assets'
S='Assets/_Project/Scenes'
for n in list(os.listdir(A)):
    if n.endswith('.unity') or n.endswith('.unity.meta'):
        shutil.move(os.path.join(A,n),os.path.join(S,n)); print('mv',n)
for d in ['Scenes','Scripts']:
    p=os.path.join(A,d)
    try:
        os.rmdir(p); print('rmdir',d)
    except Exception as e:
        print('rmdir fail',d,e)
    m=p+'.meta'
    if os.path.exists(m):
        os.remove(m); print('rmmeta',d)
