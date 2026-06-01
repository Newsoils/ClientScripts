#!/usr/bin/python3

import sys
import os
import re
import binascii
import json

class ProtoParser:
    def __init__(self, outdir):
        self.maps = {}
        self.ids = set()
        self.outdir = outdir

    def reg(self, namespace, id, name, module, cross):
        if id in self.ids:
            print("error: id 重复!!!, id={0},name={1}".format(id, name))
            raise

        if namespace not in self.maps:
            self.maps[namespace] = []
        self.ids.add(id)
        if module == "":
            self.maps[namespace].append({ "id": id, "name": name});
        else:
            self.maps[namespace].append({ "id": id, "name": name, "module": module, "cross": cross});

    def dump(self):
        with open(os.path.join(self.outdir,"all.json"), "w") as af:
            outdata = json.dumps(self.maps, ensure_ascii=False, indent=2)
            af.write(outdata)
            af.flush()
            af.close()
        nameSpaces = set()
        messageNames = set()
        for k, v in self.maps.items():
            print('v=',v)
            filename = os.path.join(self.outdir, k + ".json")
            nameSpaces.add(k)
            with open(filename, "w") as fd:
                temparrs = []
                for v1 in v:
                    temparrs.append({ "id": v1['id'], "name": v1['name']})
                    messageNames.add(v1['name'])
                outdata = json.dumps(temparrs, ensure_ascii=False, indent=2)
                fd.write(outdata)
                fd.flush()
                fd.close()
        goFilename = os.path.join("InstanceRegister.go")
        nameSpacesList = list()
        for ns in nameSpaces:
                nameSpacesList.append(ns)
        nameSpacesList.sort()
        messageNamesList = list()
        for mn in messageNames:
                messageNamesList.append(mn)
        messageNamesList.sort()
        with open(goFilename, "w") as fd:
            fd.write("package msgregist\n\n")
            fd.write("import (\n")
            for ns in nameSpacesList:
                fd.write("\t\"github.com/xcompany/xgame/proto/"+ns+"\"\n")
            fd.write(")\n\n")
            fd.write("func (msgMap *MsgNameIdMap) RegisterMsgStructName() {\n")
            for mn in messageNamesList:
                fd.write("\tmsgMap.RegisterStruct((*"+mn+")(nil))\n")
            fd.write("}\n")
            fd.flush()
            fd.close()
            
        
#for e in v:
#                print("{0}, {1} = {2}".format(k, e['id'], e['name']))

    def ParseFile(self, filename):
        f = open(filename, 'r', encoding='utf-8')
        try:
            allText = f.read()
        finally:
            f.close()
        matchObj = re.search(r'package[ \t]+(\w+);', allText, re.M)
        if matchObj:
            namespace = matchObj.group(1)
        seqObj = re.search(r'//SEQ\[+(.*),(.*)\]', allText, re.M)
        if seqObj:
            minSeq = int(seqObj.group(1))
            maxSeq = int(seqObj.group(2))
        else:
            return
        msgPat = re.compile(r'message[ \t]+(\w+Req\b)|message[ \t]+(\w+Res\b)|message[ \t]+(\w+S2C\b)|message[ \t]+(\w+S2S\b)')
        finds = msgPat.findall(allText)
        #print(finds)
        if len(finds) == 0:
            return
        if namespace == "":
            return
        prefix = namespace + '.'

        idmaps = {}
        keymaps = {}
        namemaps = {}
        for strs in finds:
            for n in strs:
                if n != "":
                    fullname = prefix + n
                    if fullname.endswith('Req'):
                        key = fullname[:len(fullname)-3]
                        # print('key',key)
                        # print('fullname',fullname)
                        # crcNum = abs(binascii.crc32(fullname.encode(encoding="utf-8"))) & 0x0000ffff
                        crcNum = minSeq + 1 + minSeq % 2
                        # print('crcNum',crcNum)
                        keymaps[key]=crcNum
                    elif fullname.endswith('Res'):
                        key = fullname[:len(fullname)-3]
                        # print('Res key',key)
                        # print('Res fullname',fullname)
                        # Some protos only define *Res without matching *Req.
                        # In that case allocate a fresh base id for the pair to avoid KeyError.
                        if key not in keymaps:
                            crcNum = minSeq + 1 + minSeq % 2
                            keymaps[key] = crcNum
                        crcNum = keymaps[key] + 1
                        # print('Res crcNum',crcNum)
                    else:
                        crcNum = minSeq + minSeq % 2 + 2
                        # print('other crcNum',crcNum)
                    if crcNum > minSeq:
                        minSeq = crcNum
                    obj = {"namespace": namespace, "id": crcNum, "name": fullname, "module": "","cross": 0}
                    idmaps[crcNum] = obj
                    namemaps[fullname] = obj
            # self.reg(namespace, crcNum, fullname)

        # print("idmaps1=",idmaps)
        msgPat = re.compile(r'message[ \t]+(\w+).*(//module=)(\w+).*')
        finds = msgPat.findall(allText)
        # print("finds=",finds)
        # if len(finds) == 0:
        #     return
        for n in finds:
            print(n)
            fullname = prefix + n[0]
            moduleName = n[2]
            if moduleName.endswith('+'):
                moduleName = moduleName[:len(moduleName)-1]
            namemaps[fullname]["module"] = moduleName
            namemaps[fullname]["cross"] = 1


        # print("idmaps2=",idmaps)
        for k, v in idmaps.items():
            self.reg(v["namespace"], v["id"], v["name"],v["module"],v["cross"])


def getFilesByExt(path, ext, out):
    files = os.listdir(path)
    for f in files:
        fullpath = path + '/' + f
        if f[0] == '.':
            continue
        if (os.path.isfile(fullpath)):
            if fullpath.endswith(ext):
                out.append(fullpath)
        else:
#print("fullpath:", fullpath)
            subfiles = os.listdir(fullpath)
            getFilesByExt(fullpath, ext, out)

def GetFilesByExt(path, ext):
    out_files = []
    getFilesByExt(path, ext, out_files)
    return out_files


if __name__ == "__main__":
    path = "."
    if len(sys.argv) <= 1:
        path = "."
    else:
        path = sys.argv[1]

    outdir = "."
    if len(sys.argv) <= 2:
        outdir = "."
    else:
        outdir = sys.argv[2]

    protofiles = GetFilesByExt(path, ".proto")

    parser = ProtoParser(outdir)
    for f in protofiles:
        print("processing: ", f)
        parser.ParseFile(f)
    parser.dump()
    print("export surccess!")

